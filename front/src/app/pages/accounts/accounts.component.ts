import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { FinanceService } from '../../core/services/finance.service';
import { Account } from '../../core/models/account.model';
import { AccountType } from '../../core/models/account-type.model';
import { Expense, Income, Reimbursement } from '../../core/models/movement.model';
import { Transfer } from '../../core/models/transfer.model';
import { Pocket } from '../../core/models/pocket.model';

interface PocketView {
  id: number;
  name: string;
  balance: number;
}

interface AccountView {
  id: number;
  name: string;
  typeName: string;
  balance: number;
  income: number;
  expense: number;
  transferNet: number;
  pockets: PocketView[];
  assigned: number;
  unassigned: number;
}

interface TransferForm {
  fromAccountId: number | null;
  toAccountId: number | null;
  amount: number | null;
  date: string;
  description: string;
}

interface PocketForm {
  accountId: number | null;
  name: string;
  amount: number | null;
}

interface MoveForm {
  accountId: number | null;
  fromId: number | null; // null = sin asignar
  toId: number | null; // null = sin asignar
  amount: number | null;
}

@Component({
  selector: 'app-accounts',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="mb-6 flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-bold text-bm-text">Cuentas</h1>
        <p class="text-sm text-bm-muted">Posición general · dentro de cada cuenta puedes repartir el dinero en pockets</p>
      </div>
      <div class="flex items-center gap-3">
        <div class="mt-card px-5 py-3 text-right">
          <div class="text-xs uppercase tracking-wide text-bm-muted">Saldo total</div>
          <div class="text-xl font-bold" [class.text-bm-success]="totalBalance() >= 0" [class.text-bm-danger]="totalBalance() < 0">
            {{ totalBalance() | number: '1.2-2' }} €
          </div>
        </div>
        <button class="mt-btn-cyan" (click)="openTransfer()" [disabled]="accounts().length < 2">
          <i class="pi pi-arrow-right-arrow-left"></i> Transferir
        </button>
      </div>
    </div>

    <!-- Tarjetas de cuentas con pockets -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <div *ngFor="let a of accountViews()" class="mt-card flex flex-col p-5">
        <div class="flex items-start justify-between">
          <div class="overflow-hidden">
            <div class="truncate text-base font-semibold text-bm-text">{{ a.name }}</div>
            <div class="text-xs text-bm-muted">{{ a.typeName }}</div>
          </div>
          <i class="pi pi-wallet text-bm-cyan"></i>
        </div>
        <div class="mt-4 text-2xl font-bold" [class.text-bm-success]="a.balance >= 0" [class.text-bm-danger]="a.balance < 0">
          {{ a.balance | number: '1.2-2' }} €
        </div>
        <div class="mt-2 flex items-center gap-3 text-xs text-bm-muted">
          <span class="text-bm-success">+{{ a.income | number: '1.2-2' }} €</span>
          <span class="text-bm-danger">−{{ a.expense | number: '1.2-2' }} €</span>
          <span *ngIf="a.transferNet !== 0" [class.text-bm-success]="a.transferNet > 0" [class.text-bm-danger]="a.transferNet < 0">
            ⇄ {{ a.transferNet > 0 ? '+' : '' }}{{ a.transferNet | number: '1.2-2' }} €
          </span>
        </div>

        <!-- Pockets -->
        <div class="mt-4 border-t border-bm-border pt-3">
          <div class="mb-2 flex items-center justify-between">
            <span class="text-xs font-semibold uppercase tracking-wide text-bm-muted">Pockets</span>
            <div class="flex gap-2">
              <button class="mt-btn-ghost text-xs" (click)="openPocket(a.id)"><i class="pi pi-plus"></i> Pocket</button>
              <button class="mt-btn-ghost text-xs" (click)="openMove(a.id)" [disabled]="a.pockets.length === 0"><i class="pi pi-arrows-h"></i> Mover</button>
            </div>
          </div>

          <div class="space-y-1.5">
            <div *ngFor="let p of a.pockets" class="flex items-center gap-2 text-sm">
              <i class="pi pi-inbox text-xs text-bm-muted"></i>
              <span class="flex-1 truncate text-bm-text">{{ p.name }}</span>
              <span class="font-medium" [class.text-bm-text]="p.balance >= 0" [class.text-bm-danger]="p.balance < 0">
                {{ p.balance | number: '1.2-2' }} €
              </span>
              <button class="text-bm-muted transition-colors hover:text-bm-danger" (click)="removePocket(p)"><i class="pi pi-trash text-xs"></i></button>
            </div>
            <div class="flex items-center gap-2 text-sm">
              <i class="pi pi-wallet text-xs text-bm-muted"></i>
              <span class="flex-1 truncate text-bm-muted">Sin asignar</span>
              <span class="font-medium" [class.text-bm-muted]="a.unassigned >= 0" [class.text-bm-danger]="a.unassigned < 0">{{ a.unassigned | number: '1.2-2' }} €</span>
            </div>
          </div>
        </div>
      </div>

      <div *ngIf="accountViews().length === 0" class="mt-card p-8 text-center text-sm text-bm-muted sm:col-span-2 lg:col-span-3">
        No tienes cuentas todavía. Créalas en <span class="text-bm-cyan">Ajustes</span>.
      </div>
    </div>

    <!-- Modal transferencia -->
    <div *ngIf="showTransfer()" class="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/60 p-4" (click)="showTransfer.set(false)">
      <div class="mt-card w-full max-w-md p-6 sm:my-auto" (click)="$event.stopPropagation()">
        <div class="mb-4 flex items-center justify-between">
          <h2 class="text-lg font-semibold text-bm-text">Nueva transferencia</h2>
          <button class="text-bm-muted hover:text-bm-text" (click)="showTransfer.set(false)"><i class="pi pi-times"></i></button>
        </div>
        <label class="mt-label">Desde</label>
        <select class="mt-input mb-3" [(ngModel)]="tForm.fromAccountId">
          <option [ngValue]="null" disabled>Cuenta de origen</option>
          <option *ngFor="let a of accounts()" [ngValue]="a.id">{{ a.name }}</option>
        </select>
        <label class="mt-label">Hacia</label>
        <select class="mt-input mb-3" [(ngModel)]="tForm.toAccountId">
          <option [ngValue]="null" disabled>Cuenta de destino</option>
          <option *ngFor="let a of accounts()" [ngValue]="a.id">{{ a.name }}</option>
        </select>
        <label class="mt-label">Importe (€)</label>
        <input class="mt-input mb-3" type="number" [(ngModel)]="tForm.amount" />
        <label class="mt-label">Fecha</label>
        <input class="mt-input mb-3" type="date" [(ngModel)]="tForm.date" />
        <label class="mt-label">Descripción</label>
        <input class="mt-input mb-4" type="text" [(ngModel)]="tForm.description" placeholder="Opcional" />
        <button class="mt-btn-cyan w-full" (click)="saveTransfer()"><i class="pi pi-save"></i> Guardar transferencia</button>
      </div>
    </div>

    <!-- Modal crear pocket -->
    <div *ngIf="showPocket()" class="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/60 p-4" (click)="showPocket.set(false)">
      <div class="mt-card w-full max-w-md p-6 sm:my-auto" (click)="$event.stopPropagation()">
        <div class="mb-4 flex items-center justify-between">
          <h2 class="text-lg font-semibold text-bm-text">Nuevo pocket</h2>
          <button class="text-bm-muted hover:text-bm-text" (click)="showPocket.set(false)"><i class="pi pi-times"></i></button>
        </div>
        <label class="mt-label">Nombre</label>
        <input class="mt-input mb-3" type="text" [(ngModel)]="pForm.name" placeholder="Ahorro, Vacaciones..." />
        <label class="mt-label">Importe inicial (€) <span class="text-bm-muted">— se coge de «sin asignar»</span></label>
        <input class="mt-input mb-4" type="number" [(ngModel)]="pForm.amount" />
        <button class="mt-btn-cyan w-full" (click)="savePocket()"><i class="pi pi-save"></i> Crear pocket</button>
      </div>
    </div>

    <!-- Modal mover dinero entre pockets -->
    <div *ngIf="showMove()" class="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/60 p-4" (click)="showMove.set(false)">
      <div class="mt-card w-full max-w-md p-6 sm:my-auto" (click)="$event.stopPropagation()">
        <div class="mb-4 flex items-center justify-between">
          <h2 class="text-lg font-semibold text-bm-text">Mover dinero</h2>
          <button class="text-bm-muted hover:text-bm-text" (click)="showMove.set(false)"><i class="pi pi-times"></i></button>
        </div>
        <label class="mt-label">Desde</label>
        <select class="mt-input mb-3" [(ngModel)]="mForm.fromId">
          <option [ngValue]="null">Sin asignar</option>
          <option *ngFor="let p of movePockets()" [ngValue]="p.id">{{ p.name }} ({{ p.balance | number: '1.2-2' }} €)</option>
        </select>
        <label class="mt-label">Hacia</label>
        <select class="mt-input mb-3" [(ngModel)]="mForm.toId">
          <option [ngValue]="null">Sin asignar</option>
          <option *ngFor="let p of movePockets()" [ngValue]="p.id">{{ p.name }} ({{ p.balance | number: '1.2-2' }} €)</option>
        </select>
        <label class="mt-label">Importe (€)</label>
        <input class="mt-input mb-4" type="number" [(ngModel)]="mForm.amount" />
        <button class="mt-btn-cyan w-full" (click)="saveMove()"><i class="pi pi-save"></i> Mover</button>
      </div>
    </div>
  `,
})
export class AccountsComponent implements OnInit {
  private finance = inject(FinanceService);
  private msg = inject(MessageService);

  accounts = signal<Account[]>([]);
  accountTypes = signal<AccountType[]>([]);
  incomes = signal<Income[]>([]);
  expenses = signal<Expense[]>([]);
  reimbursements = signal<Reimbursement[]>([]);
  transfers = signal<Transfer[]>([]);
  pockets = signal<Pocket[]>([]);

  showTransfer = signal(false);
  showPocket = signal(false);
  showMove = signal(false);
  tForm: TransferForm = this.emptyTransfer();
  pForm: PocketForm = { accountId: null, name: '', amount: null };
  mForm: MoveForm = { accountId: null, fromId: null, toId: null, amount: null };

  accountViews = computed<AccountView[]>(() => {
    const inc = new Map<number, number>();
    const exp = new Map<number, number>();
    const trIn = new Map<number, number>();
    const trOut = new Map<number, number>();
    const add = (m: Map<number, number>, id: number, v: number) => m.set(id, (m.get(id) ?? 0) + v);
    for (const i of this.incomes()) add(inc, i.accountId, Number(i.amount));
    for (const x of this.expenses()) add(exp, x.accountId, Number(x.amount));
    for (const t of this.transfers()) {
      add(trOut, t.fromAccountId, Number(t.amount));
      add(trIn, t.toAccountId, Number(t.amount));
    }
    // Gastos que adelantaste: el `amount` del gasto es solo tu parte, así que de la
    // cuenta con la que pagaste falta por descontar lo que pusiste de los demás.
    // Eso sale siempre; lo que te devuelven entra en su cuenta solo al cobrarlo.
    const expenseById = new Map(this.expenses().map((e) => [e.id, e]));
    for (const r of this.reimbursements()) {
      const paidFrom = expenseById.get(r.expenseId)?.accountId;
      if (paidFrom != null) add(trOut, paidFrom, Number(r.amount));
      if (r.settledAt) add(trIn, r.toAccountId, Number(r.amount));
    }
    const pocketsByAcc = new Map<number, PocketView[]>();
    for (const p of this.pockets()) {
      const arr = pocketsByAcc.get(p.accountId) ?? [];
      arr.push({ id: p.id, name: p.name, balance: Number(p.balance) });
      pocketsByAcc.set(p.accountId, arr);
    }

    return this.accounts().map((a) => {
      const income = inc.get(a.id) ?? 0;
      const expense = exp.get(a.id) ?? 0;
      const transferNet = (trIn.get(a.id) ?? 0) - (trOut.get(a.id) ?? 0);
      const balance = Number(a.balance) + income - expense + transferNet;
      const pockets = pocketsByAcc.get(a.id) ?? [];
      const assigned = pockets.reduce((s, p) => s + p.balance, 0);
      return {
        id: a.id,
        name: a.name,
        typeName: this.typeName(a.accountTypeId),
        balance,
        income,
        expense,
        transferNet,
        pockets,
        assigned,
        unassigned: balance - assigned,
      };
    });
  });

  totalBalance = computed(() => this.accountViews().reduce((s, a) => s + a.balance, 0));

  movePockets = computed<PocketView[]>(() => {
    const id = this.mForm.accountId;
    if (id == null) return [];
    return this.accountViews().find((a) => a.id === id)?.pockets ?? [];
  });

  ngOnInit(): void {
    this.load();
    this.finance.getAccountTypes().subscribe({ next: (r) => this.accountTypes.set(r) });
  }

  load(): void {
    this.finance.getAccounts().subscribe({ next: (r) => this.accounts.set(r) });
    this.finance.getIncomes().subscribe({ next: (r) => this.incomes.set(r) });
    this.finance.getExpenses().subscribe({ next: (r) => this.expenses.set(r) });
    this.finance.getTransfers().subscribe({ next: (r) => this.transfers.set(r) });
    this.finance.getReimbursements().subscribe({ next: (r) => this.reimbursements.set(r) });
    this.finance.getPockets().subscribe({ next: (r) => this.pockets.set(r) });
  }

  // --- Transferencias ---
  openTransfer(): void {
    this.tForm = this.emptyTransfer();
    this.showTransfer.set(true);
  }

  saveTransfer(): void {
    const f = this.tForm;
    if (!f.fromAccountId || !f.toAccountId || !f.amount) {
      this.msg.add({ severity: 'warn', summary: 'Faltan datos', detail: 'Elige origen, destino e importe' });
      return;
    }
    if (f.fromAccountId === f.toAccountId) {
      this.msg.add({ severity: 'warn', summary: 'Cuentas iguales', detail: 'Origen y destino deben ser distintos' });
      return;
    }
    this.finance
      .createTransfer({ fromAccountId: f.fromAccountId, toAccountId: f.toAccountId, amount: f.amount, transferDate: f.date, description: f.description || undefined })
      .subscribe({
        next: () => {
          this.showTransfer.set(false);
          this.msg.add({ severity: 'success', summary: 'Transferencia realizada', detail: '' });
          this.load();
        },
        error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo hacer la transferencia' }),
      });
  }

  // --- Pockets ---
  openPocket(accountId: number): void {
    this.pForm = { accountId, name: '', amount: null };
    this.showPocket.set(true);
  }

  savePocket(): void {
    const f = this.pForm;
    if (!f.accountId || !f.name.trim()) {
      this.msg.add({ severity: 'warn', summary: 'Faltan datos', detail: 'El pocket necesita un nombre' });
      return;
    }
    const amount = Number(f.amount) || 0;
    // Sin comprobar fondos: lo que no haya se queda como "sin asignar" en negativo.
    this.finance.createPocket({ accountId: f.accountId, name: f.name.trim(), balance: amount }).subscribe({
      next: () => {
        this.showPocket.set(false);
        this.msg.add({ severity: 'success', summary: 'Pocket creado', detail: '' });
        this.load();
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo crear el pocket' }),
    });
  }

  removePocket(p: PocketView): void {
    if (!confirm(`¿Eliminar el pocket "${p.name}"? Su saldo (${p.balance} €) volverá a «sin asignar».`)) return;
    this.finance.deletePocket(p.id).subscribe({
      next: () => {
        this.msg.add({ severity: 'info', summary: 'Pocket eliminado', detail: '' });
        this.load();
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo eliminar' }),
    });
  }

  openMove(accountId: number): void {
    this.mForm = { accountId, fromId: null, toId: null, amount: null };
    this.showMove.set(true);
  }

  saveMove(): void {
    const f = this.mForm;
    if (!f.amount || f.amount <= 0) {
      this.msg.add({ severity: 'warn', summary: 'Importe inválido', detail: 'Indica un importe mayor que 0' });
      return;
    }
    if (f.fromId === f.toId) {
      this.msg.add({ severity: 'warn', summary: 'Origen = destino', detail: 'Elige origen y destino distintos' });
      return;
    }
    // Sin comprobar fondos: el origen puede quedarse en negativo y ya lo cuadras luego.
    this.finance.movePocket({ fromPocketId: f.fromId, toPocketId: f.toId, amount: f.amount }).subscribe({
      next: () => {
        this.showMove.set(false);
        this.msg.add({ severity: 'success', summary: 'Dinero movido', detail: '' });
        this.load();
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo mover el dinero' }),
    });
  }

  private typeName(id: number): string {
    return this.accountTypes().find((t) => t.id === id)?.name ?? '';
  }

  private emptyTransfer(): TransferForm {
    return { fromAccountId: null, toAccountId: null, amount: null, date: new Date().toISOString().substring(0, 10), description: '' };
  }
}
