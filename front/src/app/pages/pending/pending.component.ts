import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { forkJoin } from 'rxjs';
import { FinanceService } from '../../core/services/finance.service';
import { Expense, Reimbursement } from '../../core/models/movement.model';
import { Account } from '../../core/models/account.model';

/** Una deuda concreta: el reembolso más los datos del gasto que la originó. */
interface DebtRow {
  reimbursement: Reimbursement;
  description: string;
  date: string;
  toAccountName: string;
}

/** Todo lo que te debe una misma persona. */
interface PersonDebt {
  personId: number | null;
  name: string;
  total: number;
  rows: DebtRow[];
}

@Component({
  selector: 'app-pending',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="mb-6 flex flex-wrap items-center justify-between gap-3">
      <div>
        <h1 class="text-2xl font-bold text-bm-text">Cobros pendientes</h1>
        <p class="text-sm text-bm-muted">
          Lo que adelantaste de otra gente y todavía no te han devuelto
        </p>
      </div>
      <div class="mt-card px-5 py-3 text-right">
        <div class="text-xs uppercase tracking-wide text-bm-muted">Te deben</div>
        <div class="text-xl font-bold text-bm-warning">{{ grandTotal() | number: '1.2-2' }} €</div>
      </div>
    </div>

    <label class="mb-5 flex cursor-pointer items-center gap-2 text-sm text-bm-muted">
      <input type="checkbox" class="h-4 w-4 accent-bm-cyan" [(ngModel)]="showSettled" (ngModelChange)="onToggleSettled()" />
      Ver también lo ya cobrado
    </label>

    <!-- Una tarjeta por persona -->
    <div class="space-y-4">
      <div *ngFor="let p of groups()" class="mt-card overflow-hidden">
        <div class="flex items-center justify-between border-b border-bm-border px-5 py-3">
          <div class="flex items-center gap-3">
            <div class="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-bm-surface-2 text-xs font-semibold text-bm-cyan">
              {{ initials(p.name) }}
            </div>
            <div>
              <div class="text-sm font-semibold text-bm-text">{{ p.name }}</div>
              <div class="text-xs text-bm-muted">
                {{ p.rows.length }} {{ p.rows.length === 1 ? 'gasto' : 'gastos' }}
              </div>
            </div>
          </div>
          <div class="flex items-center gap-3">
            <span class="text-base font-bold text-bm-warning">{{ p.total | number: '1.2-2' }} €</span>
            <button *ngIf="pendingCount(p) > 0" class="mt-btn-ghost text-xs" (click)="settleAll(p)">
              <i class="pi pi-check-circle"></i> Cobrar todo
            </button>
          </div>
        </div>

        <div *ngFor="let r of p.rows" class="flex items-center gap-3 border-b border-bm-border px-5 py-3 last:border-0">
          <div class="flex-1 overflow-hidden">
            <div class="truncate text-sm font-medium text-bm-text">{{ r.description }}</div>
            <div class="text-xs text-bm-muted">
              {{ r.date | slice: 0:10 }} · entra en {{ r.toAccountName }}
            </div>
          </div>
          <div class="text-sm font-semibold"
               [class.text-bm-warning]="!r.reimbursement.settledAt"
               [class.text-bm-success]="r.reimbursement.settledAt">
            +{{ r.reimbursement.amount | number: '1.2-2' }} €
          </div>
          <button *ngIf="!r.reimbursement.settledAt" class="mt-btn-ghost text-xs" (click)="settle(r.reimbursement)">
            <i class="pi pi-check"></i> Cobrado
          </button>
          <button *ngIf="r.reimbursement.settledAt" class="text-xs text-bm-muted hover:text-bm-cyan"
                  (click)="unsettle(r.reimbursement)">
            Deshacer
          </button>
        </div>
      </div>

      <div *ngIf="groups().length === 0" class="mt-card px-5 py-10 text-center text-sm text-bm-muted">
        No te debe nada nadie. Los cobros aparecen aquí cuando creas un gasto compartido
        eligiendo <span class="text-bm-cyan">Lo adelanto yo</span>.
      </div>
    </div>
  `,
})
export class PendingComponent implements OnInit {
  private finance = inject(FinanceService);
  private msg = inject(MessageService);

  reimbursements = signal<Reimbursement[]>([]);
  expenses = signal<Expense[]>([]);
  accounts = signal<Account[]>([]);
  showSettled = false;
  private settledVisible = signal(false);

  private expenseById = computed(() => new Map(this.expenses().map((e) => [e.id, e])));

  /** Solo lo pendiente, que es lo que suma como deuda viva. */
  private pending = computed(() => this.reimbursements().filter((r) => !r.settledAt));
  grandTotal = computed(() => this.pending().reduce((s, r) => s + Number(r.amount), 0));

  /** Deudas agrupadas por persona, de mayor a menor. */
  groups = computed<PersonDebt[]>(() => {
    const visible = this.settledVisible() ? this.reimbursements() : this.pending();

    const byPerson = new Map<string, PersonDebt>();
    for (const r of visible) {
      const name = r.debtor?.trim() || 'Sin especificar';
      // Agrupamos por persona; si no la indicaste, todas caen en el mismo saco.
      const key = r.personId != null ? `p${r.personId}` : `n:${name}`;

      const group = byPerson.get(key) ?? { personId: r.personId ?? null, name, total: 0, rows: [] };
      const expense = this.expenseById().get(r.expenseId);
      group.rows.push({
        reimbursement: r,
        description: expense?.description || '(sin descripción)',
        date: expense?.expenseDate ?? '',
        toAccountName: this.accountName(r.toAccountId),
      });
      // El total de la cabecera es lo que te debe de verdad, sin contar lo ya cobrado.
      if (!r.settledAt) group.total += Number(r.amount);
      byPerson.set(key, group);
    }

    for (const g of byPerson.values()) {
      g.rows.sort((a, b) => (b.date || '').localeCompare(a.date || ''));
    }

    return [...byPerson.values()].sort((a, b) => b.total - a.total);
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.finance.getReimbursements().subscribe({ next: (r) => this.reimbursements.set(r) });
    this.finance.getExpenses().subscribe({ next: (r) => this.expenses.set(r) });
    this.finance.getAccounts().subscribe({ next: (r) => this.accounts.set(r) });
  }

  onToggleSettled(): void {
    this.settledVisible.set(this.showSettled);
  }

  accountName(id: number): string {
    return this.accounts().find((a) => a.id === id)?.name ?? `cuenta ${id}`;
  }

  initials(name: string): string {
    return name.split(' ').map((p) => p[0]).slice(0, 2).join('').toUpperCase();
  }

  pendingCount(p: PersonDebt): number {
    return p.rows.filter((r) => !r.reimbursement.settledAt).length;
  }

  settle(r: Reimbursement): void {
    this.finance.settleReimbursement(r.id).subscribe({
      next: () => {
        this.msg.add({
          severity: 'success',
          summary: 'Cobrado',
          detail: `${Number(r.amount).toFixed(2)} € en ${this.accountName(r.toAccountId)}`,
        });
        this.load();
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo marcar como cobrado' }),
    });
  }

  unsettle(r: Reimbursement): void {
    this.finance.unsettleReimbursement(r.id).subscribe({
      next: () => this.load(),
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo deshacer' }),
    });
  }

  /** Te lo ha devuelto todo de una vez: marca cobrados todos sus pendientes. */
  settleAll(p: PersonDebt): void {
    const pending = p.rows.filter((r) => !r.reimbursement.settledAt);
    if (pending.length === 0) return;

    forkJoin(pending.map((r) => this.finance.settleReimbursement(r.reimbursement.id))).subscribe({
      next: () => {
        this.msg.add({
          severity: 'success',
          summary: 'Cobrado',
          detail: `${p.total.toFixed(2)} € de ${p.name}`,
        });
        this.load();
      },
      error: () => {
        this.msg.add({ severity: 'error', summary: 'Error', detail: 'Algún cobro no se pudo marcar' });
        this.load();
      },
    });
  }
}
