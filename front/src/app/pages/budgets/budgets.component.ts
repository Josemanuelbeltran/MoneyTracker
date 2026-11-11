import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { FinanceService } from '../../core/services/finance.service';
import { Expense } from '../../core/models/movement.model';
import { Category } from '../../core/models/category.model';
import { Budget } from '../../core/models/budget.model';
import { CalcAmountComponent } from '../../shared/calc-amount.component';

interface BudgetRow {
  id: number;
  categoryId: number;
  name: string;
  limit: number;
  spent: number;
  pct: number; // 0..100+ (recortado a 100 para la barra)
  rawPct: number;
  remaining: number;
  over: boolean;
}

interface BudgetGroup {
  rootId: number;
  rootName: string;
  rows: BudgetRow[];
  limit: number;
  spent: number;
  pct: number;
  rawPct: number;
  remaining: number;
  over: boolean;
}

interface Form {
  id: number | null;
  categoryId: number | null;
  limit: number | null;
  fixedName: string; // al editar, nombre de la categoría (no editable)
}

@Component({
  selector: 'app-budgets',
  standalone: true,
  imports: [CommonModule, FormsModule, CalcAmountComponent],
  template: `
    <div class="mb-6 flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-bold text-bm-text">Presupuestos</h1>
        <p class="text-sm text-bm-muted">Presupuesto mensual por categoría vs gasto real del mes elegido</p>
      </div>
      <div class="flex items-end gap-3">
        <div>
          <label class="mb-1 block text-xs text-bm-muted">Mes</label>
          <input class="mt-input" type="month" [ngModel]="month()" (ngModelChange)="month.set($event)" />
        </div>
        <button class="mt-btn-cyan" (click)="openNew()"><i class="pi pi-plus"></i> Nuevo presupuesto</button>
      </div>
    </div>

    <!-- Resumen -->
    <div class="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-3">
      <div class="mt-card p-5">
        <div class="text-xs uppercase tracking-wide text-bm-muted">Presupuestado</div>
        <div class="mt-2 text-2xl font-bold text-bm-text">{{ totalLimit() | number: '1.2-2' }} €</div>
      </div>
      <div class="mt-card p-5">
        <div class="text-xs uppercase tracking-wide text-bm-muted">Gastado</div>
        <div class="mt-2 text-2xl font-bold text-bm-danger">{{ totalSpent() | number: '1.2-2' }} €</div>
      </div>
      <div class="mt-card p-5">
        <div class="text-xs uppercase tracking-wide text-bm-muted">Disponible</div>
        <div class="mt-2 text-2xl font-bold" [class.text-bm-success]="totalRemaining() >= 0" [class.text-bm-danger]="totalRemaining() < 0">
          {{ totalRemaining() | number: '1.2-2' }} €
        </div>
      </div>
    </div>

    <!-- Lista de presupuestos, agrupada por categoría padre (plegable) -->
    <div class="space-y-3">
      <div *ngFor="let g of groups()" class="mt-card overflow-hidden">
        <!-- Cabecera de grupo (categoría padre) -->
        <button (click)="toggle(g.rootId)" class="flex w-full items-center gap-3 px-5 pt-4 text-left">
          <i class="pi text-xs text-bm-muted" [ngClass]="isOpen(g.rootId) ? 'pi-chevron-down' : 'pi-chevron-right'"></i>
          <span class="flex-1 text-sm font-semibold text-bm-text">{{ g.rootName }}</span>
          <span class="text-sm" [class.text-bm-danger]="g.over" [class.text-bm-muted]="!g.over">
            {{ g.spent | number: '1.2-2' }} € / {{ g.limit | number: '1.2-2' }} €
          </span>
        </button>
        <div class="px-5 pb-4 pt-2">
          <div class="h-2 w-full overflow-hidden rounded-full bg-bm-surface-2">
            <div class="h-full rounded-full transition-all" [style.width.%]="g.pct" [style.background-color]="barColor(g.rawPct)"></div>
          </div>
        </div>

        <!-- Detalle: cada presupuesto del grupo -->
        <div *ngIf="isOpen(g.rootId)" class="border-t border-bm-border">
          <div *ngFor="let r of g.rows" class="px-5 py-4">
            <div class="mb-2 flex items-center justify-between gap-2">
              <span class="text-sm text-bm-text">{{ r.name }}</span>
              <div class="flex items-center gap-3">
                <span class="text-sm" [class.text-bm-danger]="r.over" [class.text-bm-muted]="!r.over">
                  {{ r.spent | number: '1.2-2' }} € / {{ r.limit | number: '1.2-2' }} €
                </span>
                <button class="text-bm-muted transition-colors hover:text-bm-cyan" (click)="openEdit(r)"><i class="pi pi-pencil text-xs"></i></button>
                <button class="text-bm-muted transition-colors hover:text-bm-danger" (click)="remove(r)"><i class="pi pi-trash text-xs"></i></button>
              </div>
            </div>
            <div class="h-1.5 w-full overflow-hidden rounded-full bg-bm-surface-2">
              <div class="h-full rounded-full transition-all" [style.width.%]="r.pct" [style.background-color]="barColor(r.rawPct)"></div>
            </div>
            <div class="mt-1.5 text-xs" [class.text-bm-danger]="r.over" [class.text-bm-muted]="!r.over">
              <span *ngIf="!r.over">Te queda {{ r.remaining | number: '1.2-2' }} € ({{ r.rawPct | number: '1.0-0' }}%)</span>
              <span *ngIf="r.over">Te has pasado {{ -r.remaining | number: '1.2-2' }} € ({{ r.rawPct | number: '1.0-0' }}%)</span>
            </div>
          </div>
        </div>
      </div>

      <div *ngIf="groups().length === 0" class="mt-card px-5 py-10 text-center text-sm text-bm-muted">
        No tienes presupuestos. Pulsa «Nuevo presupuesto» para crear uno.
      </div>
    </div>

    <!-- Modal crear/editar -->
    <div *ngIf="showForm()" class="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/60 p-4" (click)="showForm.set(false)">
      <div class="mt-card w-full max-w-md p-6 sm:my-auto" (click)="$event.stopPropagation()">
        <div class="mb-4 flex items-center justify-between">
          <h2 class="text-lg font-semibold text-bm-text">{{ form.id ? 'Editar presupuesto' : 'Nuevo presupuesto' }}</h2>
          <button class="text-bm-muted hover:text-bm-text" (click)="showForm.set(false)"><i class="pi pi-times"></i></button>
        </div>

        <label class="mt-label">Categoría</label>
        <ng-container *ngIf="form.id == null; else fixedCat">
          <select class="mt-input mb-3" [(ngModel)]="form.categoryId">
            <option [ngValue]="null" disabled>Selecciona categoría</option>
            <option *ngFor="let c of availableCategories()" [ngValue]="c.id">{{ c.label || c.name }}</option>
          </select>
        </ng-container>
        <ng-template #fixedCat>
          <div class="mt-input mb-3 opacity-70">{{ form.fixedName }}</div>
        </ng-template>

        <label class="mt-label">Límite mensual (€)</label>
        <div class="mb-4"><app-calc-amount [(value)]="form.limit" /></div>

        <button class="mt-btn-cyan w-full" (click)="save()"><i class="pi pi-save"></i> Guardar</button>
      </div>
    </div>
  `,
})
export class BudgetsComponent implements OnInit {
  private finance = inject(FinanceService);
  private msg = inject(MessageService);

  budgets = signal<Budget[]>([]);
  expenses = signal<Expense[]>([]);
  cats = signal<Category[]>([]);
  month = signal<string>(new Date().toISOString().substring(0, 7));
  showForm = signal(false);
  form: Form = this.empty();

  private catIndex = computed(() => new Map(this.cats().map((c) => [c.id, c])));

  private inMonth = (date: string) => (date || '').substring(0, 7) === this.month();

  rows = computed<BudgetRow[]>(() => {
    const index = this.catIndex();
    const monthExpenses = this.expenses().filter((e) => this.inMonth(e.expenseDate));
    return this.budgets()
      .map((b) => {
        const limit = Number(b.monthlyLimit);
        const spent = monthExpenses
          .filter((e) => this.rollsUpTo(e.categoryId, b.categoryId, index))
          .reduce((s, e) => s + Number(e.amount), 0);
        const rawPct = limit > 0 ? (spent / limit) * 100 : spent > 0 ? 999 : 0;
        return {
          id: b.id,
          categoryId: b.categoryId,
          name: index.get(b.categoryId)?.name ?? `Categoría ${b.categoryId}`,
          limit,
          spent,
          pct: Math.min(100, rawPct),
          rawPct,
          remaining: limit - spent,
          over: spent > limit,
        };
      })
      .sort((a, b) => b.rawPct - a.rawPct);
  });

  /** Presupuestos agrupados por su categoría raíz (padre de más arriba). */
  groups = computed<BudgetGroup[]>(() => {
    const index = this.catIndex();
    const byRoot = new Map<number, BudgetRow[]>();
    for (const r of this.rows()) {
      const root = this.rootOf(r.categoryId, index);
      const arr = byRoot.get(root) ?? [];
      arr.push(r);
      byRoot.set(root, arr);
    }
    return [...byRoot.entries()]
      .map(([rootId, rws]) => {
        const limit = rws.reduce((s, r) => s + r.limit, 0);
        const spent = rws.reduce((s, r) => s + r.spent, 0);
        const rawPct = limit > 0 ? (spent / limit) * 100 : spent > 0 ? 999 : 0;
        return {
          rootId,
          rootName: index.get(rootId)?.name ?? `Categoría ${rootId}`,
          rows: rws,
          limit,
          spent,
          pct: Math.min(100, rawPct),
          rawPct,
          remaining: limit - spent,
          over: spent > limit,
        };
      })
      .sort((a, b) => a.rootName.localeCompare(b.rootName));
  });

  expanded = signal<Set<number>>(new Set()); // plegado por defecto

  toggle(rootId: number): void {
    this.expanded.update((set) => {
      const next = new Set(set);
      next.has(rootId) ? next.delete(rootId) : next.add(rootId);
      return next;
    });
  }

  isOpen(rootId: number): boolean {
    return this.expanded().has(rootId);
  }

  totalLimit = computed(() => this.rows().reduce((s, r) => s + r.limit, 0));
  totalSpent = computed(() => this.rows().reduce((s, r) => s + r.spent, 0));
  totalRemaining = computed(() => this.totalLimit() - this.totalSpent());

  availableCategories = computed(() => {
    const used = new Set(this.budgets().map((b) => b.categoryId));
    return this.cats().filter((c) => !used.has(c.id));
  });

  ngOnInit(): void {
    this.load();
    this.finance.getExpenseCategories().subscribe({ next: (r) => this.cats.set(r) });
  }

  load(): void {
    this.finance.getBudgets().subscribe({ next: (r) => this.budgets.set(r) });
    this.finance.getExpenses().subscribe({ next: (r) => this.expenses.set(r) });
  }

  barColor(rawPct: number): string {
    if (rawPct > 100) return '#F85149';
    if (rawPct > 80) return '#E3B341';
    return '#3FB950';
  }

  openNew(): void {
    this.form = this.empty();
    this.showForm.set(true);
  }

  openEdit(r: BudgetRow): void {
    this.form = { id: r.id, categoryId: r.categoryId, limit: r.limit, fixedName: r.name };
    this.showForm.set(true);
  }

  save(): void {
    const f = this.form;
    if (!f.categoryId || f.limit == null || f.limit < 0) {
      this.msg.add({ severity: 'warn', summary: 'Faltan datos', detail: 'Elige categoría e importe' });
      return;
    }
    this.finance.upsertBudget({ categoryId: f.categoryId, monthlyLimit: f.limit }).subscribe({
      next: () => {
        this.showForm.set(false);
        this.msg.add({ severity: 'success', summary: 'Presupuesto guardado', detail: '' });
        this.load();
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo guardar' }),
    });
  }

  remove(r: BudgetRow): void {
    if (!confirm(`¿Eliminar el presupuesto de "${r.name}"?`)) return;
    this.finance.deleteBudget(r.id).subscribe({
      next: () => {
        this.msg.add({ severity: 'info', summary: 'Presupuesto eliminado', detail: '' });
        this.load();
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo eliminar' }),
    });
  }

  /** Sube por parentId hasta la categoría raíz (la de más arriba). */
  private rootOf(catId: number, index: Map<number, Category>): number {
    let cur = catId;
    const seen = new Set<number>();
    while (!seen.has(cur)) {
      seen.add(cur);
      const parent = index.get(cur)?.parentId;
      if (parent == null) break;
      cur = parent;
    }
    return cur;
  }

  /** ¿La categoría catId es targetId o desciende de ella? (para agregar subcategorías). */
  private rollsUpTo(catId: number, targetId: number, index: Map<number, Category>): boolean {
    let cur: number | null | undefined = catId;
    const seen = new Set<number>();
    while (cur != null && !seen.has(cur)) {
      if (cur === targetId) return true;
      seen.add(cur);
      cur = index.get(cur)?.parentId ?? null;
    }
    return false;
  }

  private empty(): Form {
    return { id: null, categoryId: null, limit: null, fixedName: '' };
  }
}
