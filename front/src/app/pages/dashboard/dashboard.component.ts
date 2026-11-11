import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { FinanceService } from '../../core/services/finance.service';
import { Expense, Income } from '../../core/models/movement.model';
import { Category } from '../../core/models/category.model';

interface Slice {
  catId: number;
  name: string;
  amount: number;
  pct: number;
  color: string;
  dashArray: string;
  dashOffset: number;
  drillable: boolean;
}

interface Movement {
  categoryId: number;
  amount: number;
}

const PALETTE = ['#2EC5FF', '#3FB950', '#F0883E', '#F85149', '#A371F7', '#DB61A2', '#E3B341', '#39C5CF', '#8B949E'];
const RADIUS = 54;
const CIRC = 2 * Math.PI * RADIUS;

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="mb-6 flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-bold text-bm-text">Hola, {{ userName() || 'Usuario' }} 👋</h1>
        <p class="text-sm text-bm-muted">Estadísticas del mes</p>
      </div>
      <div>
        <label class="mb-1 block text-xs text-bm-muted">Mes</label>
        <input class="mt-input" type="month" [ngModel]="month()" (ngModelChange)="onMonthChange($event)" />
      </div>
    </div>

    <!-- KPIs del mes -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
      <div class="mt-card p-5">
        <div class="flex items-start justify-between">
          <span class="text-xs uppercase tracking-wide text-bm-muted">Ingresos</span>
          <i class="pi pi-arrow-up-right text-bm-success"></i>
        </div>
        <div class="mt-3 text-2xl font-bold text-bm-success">{{ totalIncome() | number: '1.2-2' }} €</div>
      </div>
      <div class="mt-card p-5">
        <div class="flex items-start justify-between">
          <span class="text-xs uppercase tracking-wide text-bm-muted">Gastos</span>
          <i class="pi pi-arrow-down-left text-bm-danger"></i>
        </div>
        <div class="mt-3 text-2xl font-bold text-bm-danger">{{ totalExpense() | number: '1.2-2' }} €</div>
      </div>
      <div class="mt-card p-5">
        <div class="flex items-start justify-between">
          <span class="text-xs uppercase tracking-wide text-bm-muted">Balance</span>
          <i class="pi pi-wallet text-bm-cyan"></i>
        </div>
        <div class="mt-3 text-2xl font-bold" [class.text-bm-success]="balance() >= 0" [class.text-bm-danger]="balance() < 0">
          {{ balance() | number: '1.2-2' }} €
        </div>
      </div>
    </div>

    <!-- Donuts: gastos e ingresos por categoría (con drill-down a subcategorías) -->
    <div class="mt-6 grid grid-cols-1 gap-5 lg:grid-cols-2">
      <!-- GASTOS -->
      <div class="mt-card p-5">
        <div class="mb-4 flex items-center justify-between gap-2">
          <h2 class="text-xs font-semibold uppercase tracking-wide text-bm-muted">
            {{ expenseDrill() == null ? '¿En qué gastas más?' : 'Subcategorías · ' + drillName(expenseIndex(), expenseDrill()) }}
          </h2>
          <button *ngIf="expenseDrill() != null" (click)="expenseDrill.set(null)" class="mt-btn-ghost flex items-center gap-1 text-xs">
            <i class="pi pi-arrow-left"></i> Volver
          </button>
        </div>
        <ng-container *ngTemplateOutlet="donut; context: { slices: expenseSlices(), total: sum(expenseSlices()), onClick: pickExpense }"></ng-container>
      </div>

      <!-- INGRESOS -->
      <div class="mt-card p-5">
        <div class="mb-4 flex items-center justify-between gap-2">
          <h2 class="text-xs font-semibold uppercase tracking-wide text-bm-muted">
            {{ incomeDrill() == null ? '¿De dónde vienen tus ingresos?' : 'Subcategorías · ' + drillName(incomeIndex(), incomeDrill()) }}
          </h2>
          <button *ngIf="incomeDrill() != null" (click)="incomeDrill.set(null)" class="mt-btn-ghost flex items-center gap-1 text-xs">
            <i class="pi pi-arrow-left"></i> Volver
          </button>
        </div>
        <ng-container *ngTemplateOutlet="donut; context: { slices: incomeSlices(), total: sum(incomeSlices()), onClick: pickIncome }"></ng-container>
      </div>
    </div>

    <!-- Movimientos del mes -->
    <div class="mt-6 mt-card overflow-hidden">
      <div class="border-b border-bm-border px-5 py-4">
        <h2 class="text-xs font-semibold uppercase tracking-wide text-bm-muted">Movimientos del mes</h2>
      </div>
      <div class="max-h-96 overflow-y-auto">
        <div *ngFor="let m of movements()" class="flex items-center gap-3 border-b border-bm-border px-5 py-3 last:border-0">
          <i class="pi" [ngClass]="m.kind === 'income' ? 'pi-arrow-up-right text-bm-success' : 'pi-arrow-down-left text-bm-danger'"></i>
          <div class="flex-1 overflow-hidden">
            <div class="truncate text-sm font-medium text-bm-text">{{ m.label }}</div>
            <div class="text-xs text-bm-muted">{{ m.date }}</div>
          </div>
          <div class="text-sm font-semibold" [ngClass]="m.kind === 'income' ? 'text-bm-success' : 'text-bm-danger'">
            {{ m.kind === 'income' ? '+' : '-' }}{{ m.amount | number: '1.2-2' }} €
          </div>
        </div>
        <div *ngIf="movements().length === 0" class="px-5 py-10 text-center text-sm text-bm-muted">Sin movimientos este mes.</div>
      </div>
    </div>

    <!-- Plantilla reutilizable de donut -->
    <ng-template #donut let-slices="slices" let-total="total" let-onClick="onClick">
      <div *ngIf="slices.length > 0; else noData" class="flex flex-col items-center gap-6 sm:flex-row">
        <svg viewBox="0 0 140 140" class="h-40 w-40 shrink-0">
          <g transform="rotate(-90 70 70)">
            <circle cx="70" cy="70" [attr.r]="radius" fill="none" stroke="#22272E" stroke-width="22"></circle>
            <circle
              *ngFor="let s of slices"
              cx="70" cy="70" [attr.r]="radius" fill="none"
              [attr.stroke]="s.color" stroke-width="22"
              [attr.stroke-dasharray]="s.dashArray"
              [attr.stroke-dashoffset]="s.dashOffset"
              [class.cursor-pointer]="s.drillable"
              (click)="onClick(s)">
            </circle>
          </g>
          <text x="70" y="66" text-anchor="middle" style="fill:#8B949E;font-size:9px">Total</text>
          <text x="70" y="80" text-anchor="middle" style="fill:#E6EDF3;font-size:13px;font-weight:700">{{ total | number: '1.0-0' }} €</text>
        </svg>

        <div class="flex-1 space-y-2 self-stretch">
          <div
            *ngFor="let s of slices"
            class="flex items-center gap-2 rounded px-1 py-0.5 text-sm"
            [class.cursor-pointer]="s.drillable"
            [class.hover:bg-bm-border]="s.drillable"
            (click)="onClick(s)">
            <span class="h-3 w-3 shrink-0 rounded-sm" [style.background]="s.color"></span>
            <span class="flex-1 truncate text-bm-text">{{ s.name }}<i *ngIf="s.drillable" class="pi pi-angle-right ml-1 text-xs text-bm-muted"></i></span>
            <span class="text-bm-muted">{{ s.pct | number: '1.0-0' }}%</span>
            <span class="w-20 text-right font-medium text-bm-text">{{ s.amount | number: '1.2-2' }} €</span>
          </div>
        </div>
      </div>
      <ng-template #noData>
        <div class="py-10 text-center text-sm text-bm-muted">No hay datos este mes.</div>
      </ng-template>
    </ng-template>
  `,
})
export class DashboardComponent implements OnInit {
  private finance = inject(FinanceService);
  private auth = inject(AuthService);

  readonly radius = RADIUS;

  incomes = signal<Income[]>([]);
  expenses = signal<Expense[]>([]);
  expenseCats = signal<Category[]>([]);
  incomeCats = signal<Category[]>([]);
  userName = this.auth.userName;

  month = signal<string>(new Date().toISOString().substring(0, 7)); // 'YYYY-MM'
  expenseDrill = signal<number | null>(null);
  incomeDrill = signal<number | null>(null);

  private inMonth = (date: string) => (date || '').substring(0, 7) === this.month();

  fIncomes = computed(() => this.incomes().filter((i) => this.inMonth(i.incomeDate)));
  fExpenses = computed(() => this.expenses().filter((e) => this.inMonth(e.expenseDate)));

  totalIncome = computed(() => this.fIncomes().reduce((s, i) => s + Number(i.amount), 0));
  totalExpense = computed(() => this.fExpenses().reduce((s, e) => s + Number(e.amount), 0));
  balance = computed(() => this.totalIncome() - this.totalExpense());

  // Índices catId -> Category (con parentId) para resolver raíz/subcategorías.
  expenseIndex = computed(() => this.indexOf(this.expenseCats()));
  incomeIndex = computed(() => this.indexOf(this.incomeCats()));

  expenseSlices = computed<Slice[]>(() =>
    this.makeSlices(this.fExpenses().map((e) => ({ categoryId: e.categoryId, amount: Number(e.amount) })), this.expenseIndex(), this.expenseDrill()),
  );
  incomeSlices = computed<Slice[]>(() =>
    this.makeSlices(this.fIncomes().map((i) => ({ categoryId: i.categoryId, amount: Number(i.amount) })), this.incomeIndex(), this.incomeDrill()),
  );

  movements = computed(() => {
    const items = [
      ...this.fIncomes().map((i) => ({ kind: 'income' as const, label: i.description || 'Ingreso', amount: Number(i.amount), date: (i.incomeDate || '').substring(0, 10) })),
      ...this.fExpenses().map((e) => ({ kind: 'expense' as const, label: e.description || 'Gasto', amount: Number(e.amount), date: (e.expenseDate || '').substring(0, 10) })),
    ];
    return items.sort((a, b) => b.date.localeCompare(a.date));
  });

  ngOnInit(): void {
    this.finance.getIncomes().subscribe({ next: (r) => this.incomes.set(r) });
    this.finance.getExpenses().subscribe({ next: (r) => this.expenses.set(r) });
    this.finance.getExpenseCategories().subscribe({ next: (r) => this.expenseCats.set(r) });
    this.finance.getIncomeCategories().subscribe({ next: (r) => this.incomeCats.set(r) });
  }

  onMonthChange(value: string): void {
    this.month.set(value);
    this.expenseDrill.set(null);
    this.incomeDrill.set(null);
  }

  // Arrow fns para pasarlas como callback a la plantilla del donut.
  pickExpense = (s: Slice): void => {
    if (s.drillable) this.expenseDrill.set(s.catId);
  };
  pickIncome = (s: Slice): void => {
    if (s.drillable) this.incomeDrill.set(s.catId);
  };

  sum(slices: Slice[]): number {
    return slices.reduce((s, x) => s + x.amount, 0);
  }

  drillName(index: Map<number, Category>, id: number | null): string {
    return id == null ? '' : index.get(id)?.name ?? `Categoría ${id}`;
  }

  private indexOf(cats: Category[]): Map<number, Category> {
    return new Map(cats.map((c) => [c.id, c]));
  }

  /** Sube por parentId hasta la categoría raíz. */
  private rootOf(id: number, index: Map<number, Category>): number {
    let c = index.get(id);
    const seen = new Set<number>();
    while (c && c.parentId != null && !seen.has(c.id)) {
      seen.add(c.id);
      const p = index.get(c.parentId);
      if (!p) break;
      c = p;
    }
    return c ? c.id : id;
  }

  private hasChildren(id: number, index: Map<number, Category>): boolean {
    for (const c of index.values()) if (c.parentId === id) return true;
    return false;
  }

  private makeSlices(movs: Movement[], index: Map<number, Category>, drill: number | null): Slice[] {
    const byKey = new Map<number, number>();
    for (const m of movs) {
      const root = this.rootOf(m.categoryId, index);
      if (drill == null) {
        byKey.set(root, (byKey.get(root) ?? 0) + m.amount);
      } else if (root === drill) {
        // Dentro de una raíz: agrupar por la categoría concreta del movimiento.
        byKey.set(m.categoryId, (byKey.get(m.categoryId) ?? 0) + m.amount);
      }
    }
    const total = [...byKey.values()].reduce((s, v) => s + v, 0);
    if (total <= 0) return [];

    const entries = [...byKey.entries()].sort((a, b) => b[1] - a[1]);
    let acc = 0;
    return entries.map(([catId, amount], i) => {
      const frac = amount / total;
      const len = frac * CIRC;
      const slice: Slice = {
        catId,
        name: index.get(catId)?.name ?? `Categoría ${catId}`,
        amount,
        pct: frac * 100,
        color: PALETTE[i % PALETTE.length],
        dashArray: `${len} ${CIRC - len}`,
        dashOffset: -acc,
        drillable: drill == null && this.hasChildren(catId, index),
      };
      acc += len;
      return slice;
    });
  }
}
