import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FinanceService } from '../../core/services/finance.service';
import { Expense, Income } from '../../core/models/movement.model';
import { Category } from '../../core/models/category.model';

interface MonthPoint {
  key: string; // YYYY-MM
  label: string;
  income: number;
  expense: number;
}

interface CatBar {
  name: string;
  amount: number;
  pct: number;
}

const MONTHS_ES = ['ene', 'feb', 'mar', 'abr', 'may', 'jun', 'jul', 'ago', 'sep', 'oct', 'nov', 'dic'];

@Component({
  selector: 'app-stats',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="mb-6 flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-bold text-bm-text">Estadísticas</h1>
        <p class="text-sm text-bm-muted">{{ rangeLabel() }}</p>
      </div>
    </div>

    <!-- Selector de rango de fechas -->
    <div class="mb-6 mt-card flex flex-wrap items-end gap-3 p-4">
      <div>
        <label class="mb-1 block text-xs text-bm-muted">Desde</label>
        <input class="mt-input w-40" type="month" [ngModel]="fromMonth()" (ngModelChange)="fromMonth.set($event)" />
      </div>
      <div>
        <label class="mb-1 block text-xs text-bm-muted">Hasta</label>
        <input class="mt-input w-40" type="month" [ngModel]="toMonth()" (ngModelChange)="toMonth.set($event)" />
      </div>
      <div class="flex flex-wrap gap-1.5">
        <button *ngFor="let p of presets" class="mt-btn-ghost text-xs" (click)="applyPreset(p.months)">{{ p.label }}</button>
        <button class="mt-btn-ghost text-xs" (click)="applyYear()">Este año</button>
        <button class="mt-btn-ghost text-xs" (click)="applyAll()">Todo</button>
      </div>
    </div>

    <!-- KPIs -->
    <div class="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
      <div class="mt-card p-5">
        <div class="text-xs uppercase tracking-wide text-bm-muted">Gasto total</div>
        <div class="mt-2 text-xl font-bold text-bm-danger">{{ totalExpense() | number: '1.2-2' }} €</div>
      </div>
      <div class="mt-card p-5">
        <div class="text-xs uppercase tracking-wide text-bm-muted">Ingreso total</div>
        <div class="mt-2 text-xl font-bold text-bm-success">{{ totalIncome() | number: '1.2-2' }} €</div>
      </div>
      <div class="mt-card p-5">
        <div class="text-xs uppercase tracking-wide text-bm-muted">Gasto medio/mes</div>
        <div class="mt-2 text-xl font-bold text-bm-text">{{ avgExpense() | number: '1.2-2' }} €</div>
      </div>
      <div class="mt-card p-5">
        <div class="text-xs uppercase tracking-wide text-bm-muted">Balance</div>
        <div class="mt-2 text-xl font-bold" [class.text-bm-success]="balance() >= 0" [class.text-bm-danger]="balance() < 0">{{ balance() | number: '1.2-2' }} €</div>
      </div>
    </div>

    <!-- Destacados -->
    <div class="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2">
      <div class="mt-card p-5">
        <div class="text-xs uppercase tracking-wide text-bm-muted">📈 Mes que más gastaste</div>
        <div class="mt-2 text-lg font-semibold text-bm-text">{{ topExpenseMonth()?.label || '—' }}</div>
        <div class="text-sm text-bm-danger">{{ (topExpenseMonth()?.expense || 0) | number: '1.2-2' }} €</div>
      </div>
      <div class="mt-card p-5">
        <div class="text-xs uppercase tracking-wide text-bm-muted">💰 Mes que más ingresaste</div>
        <div class="mt-2 text-lg font-semibold text-bm-text">{{ topIncomeMonth()?.label || '—' }}</div>
        <div class="text-sm text-bm-success">{{ (topIncomeMonth()?.income || 0) | number: '1.2-2' }} €</div>
      </div>
    </div>

    <!-- Ingresos vs gastos por mes -->
    <div class="mb-6 mt-card p-5">
      <div class="mb-4 flex items-center justify-between">
        <h2 class="text-xs font-semibold uppercase tracking-wide text-bm-muted">Ingresos vs gastos por mes</h2>
        <div class="flex items-center gap-3 text-xs text-bm-muted">
          <span class="flex items-center gap-1"><span class="h-2 w-2 rounded-sm bg-bm-success"></span>Ingresos</span>
          <span class="flex items-center gap-1"><span class="h-2 w-2 rounded-sm bg-bm-danger"></span>Gastos</span>
        </div>
      </div>
      <div class="flex h-48 items-end gap-1">
        <div *ngFor="let m of monthly()" class="flex h-full flex-1 flex-col items-center justify-end gap-1" [title]="m.label + ': +' + (m.income | number: '1.2-2') + ' / -' + (m.expense | number: '1.2-2')">
          <div class="flex h-full w-full items-end justify-center gap-0.5">
            <div class="w-2 rounded-t bg-bm-success sm:w-2.5" [style.height.%]="barPct(m.income)"></div>
            <div class="w-2 rounded-t bg-bm-danger sm:w-2.5" [style.height.%]="barPct(m.expense)"></div>
          </div>
          <span class="text-[10px] text-bm-muted">{{ m.label }}</span>
        </div>
      </div>
    </div>

    <!-- Gasto por mes de una categoría -->
    <div class="mb-6 mt-card p-5">
      <div class="mb-4 flex flex-wrap items-center justify-between gap-2">
        <h2 class="text-xs font-semibold uppercase tracking-wide text-bm-muted">¿Qué mes gastaste más en…?</h2>
        <select class="mt-input sm:w-56" [ngModel]="catId()" (ngModelChange)="catId.set($event)">
          <option [ngValue]="null">Todas las categorías</option>
          <option *ngFor="let c of cats()" [ngValue]="c.id">{{ c.label || c.name }}</option>
        </select>
      </div>
      <div *ngIf="catTopMonth() as t" class="mb-3 text-sm text-bm-muted">
        Máximo: <span class="font-semibold text-bm-text">{{ t.label }}</span> · <span class="text-bm-danger">{{ t.expense | number: '1.2-2' }} €</span>
      </div>
      <div class="flex h-40 items-end gap-1">
        <div *ngFor="let m of catMonthly()" class="flex h-full flex-1 flex-col items-center justify-end gap-1" [title]="m.label + ': ' + (m.expense | number: '1.2-2') + ' €'">
          <div class="w-full max-w-[16px] rounded-t bg-bm-cyan" [style.height.%]="catBarPct(m.expense)" [class.opacity-100]="m.key === catTopMonth()?.key" [class.opacity-50]="m.key !== catTopMonth()?.key"></div>
          <span class="text-[10px] text-bm-muted">{{ m.label }}</span>
        </div>
      </div>
    </div>

    <!-- Top categorías por gasto -->
    <div class="mt-card p-5">
      <h2 class="mb-4 text-xs font-semibold uppercase tracking-wide text-bm-muted">En qué gastas más (top categorías)</h2>
      <div class="space-y-2">
        <div *ngFor="let c of topCategories()" class="flex items-center gap-3">
          <span class="w-32 shrink-0 truncate text-sm text-bm-text sm:w-40">{{ c.name }}</span>
          <div class="h-4 flex-1 overflow-hidden rounded bg-bm-surface-2">
            <div class="h-full rounded bg-bm-danger" [style.width.%]="c.pct"></div>
          </div>
          <span class="w-24 shrink-0 text-right text-sm font-medium text-bm-text">{{ c.amount | number: '1.2-2' }} €</span>
        </div>
        <div *ngIf="topCategories().length === 0" class="py-6 text-center text-sm text-bm-muted">Sin gastos todavía.</div>
      </div>
    </div>
  `,
})
export class StatsComponent implements OnInit {
  private finance = inject(FinanceService);

  incomes = signal<Income[]>([]);
  expenses = signal<Expense[]>([]);
  cats = signal<Category[]>([]);
  catId = signal<number | null>(null);

  // Rango de fechas seleccionable (por defecto, los últimos 12 meses).
  fromMonth = signal<string>(this.monthsAgo(11));
  toMonth = signal<string>(this.thisMonth());
  readonly presets: { label: string; months: number }[] = [
    { label: '3m', months: 3 },
    { label: '6m', months: 6 },
    { label: '12m', months: 12 },
  ];

  private catIndex = computed(() => new Map(this.cats().map((c) => [c.id, c])));

  /** Claves 'YYYY-MM' del rango seleccionado (más antiguo → más reciente). */
  private monthKeys = computed<string[]>(() => {
    const [fy, fm] = this.fromMonth().split('-').map(Number);
    const [ty, tm] = this.toMonth().split('-').map(Number);
    let cur = new Date(fy, fm - 1, 1);
    const end = new Date(ty, tm - 1, 1);
    if (cur > end) cur = new Date(end); // rango invertido → mostramos solo el mes 'hasta'
    const keys: string[] = [];
    let guard = 0;
    while (cur <= end && guard < 600) {
      keys.push(`${cur.getFullYear()}-${String(cur.getMonth() + 1).padStart(2, '0')}`);
      cur = new Date(cur.getFullYear(), cur.getMonth() + 1, 1);
      guard++;
    }
    return keys.length ? keys : [this.toMonth()];
  });

  rangeLabel = computed<string>(() => {
    const keys = this.monthKeys();
    const n = keys.length;
    return `${this.label(keys[0])} – ${this.label(keys[n - 1])} · ${n} ${n === 1 ? 'mes' : 'meses'}`;
  });

  monthly = computed<MonthPoint[]>(() => {
    const inc = new Map<string, number>();
    const exp = new Map<string, number>();
    for (const i of this.incomes()) {
      const k = (i.incomeDate || '').substring(0, 7);
      inc.set(k, (inc.get(k) ?? 0) + Number(i.amount));
    }
    for (const e of this.expenses()) {
      const k = (e.expenseDate || '').substring(0, 7);
      exp.set(k, (exp.get(k) ?? 0) + Number(e.amount));
    }
    return this.monthKeys().map((k) => ({ key: k, label: this.label(k), income: inc.get(k) ?? 0, expense: exp.get(k) ?? 0 }));
  });

  private maxMonthly = computed(() => Math.max(1, ...this.monthly().map((m) => Math.max(m.income, m.expense))));

  totalIncome = computed(() => this.monthly().reduce((s, m) => s + m.income, 0));
  totalExpense = computed(() => this.monthly().reduce((s, m) => s + m.expense, 0));
  balance = computed(() => this.totalIncome() - this.totalExpense());
  avgExpense = computed(() => this.totalExpense() / (this.monthKeys().length || 1));

  topExpenseMonth = computed(() => this.monthly().reduce<MonthPoint | null>((best, m) => (m.expense > (best?.expense ?? -1) ? m : best), null));
  topIncomeMonth = computed(() => this.monthly().reduce<MonthPoint | null>((best, m) => (m.income > (best?.income ?? -1) ? m : best), null));

  // Serie mensual del gasto para la categoría elegida (o todas)
  catMonthly = computed<MonthPoint[]>(() => {
    const exp = new Map<string, number>();
    for (const e of this.expenses()) {
      if (!this.matchesCat(e.categoryId)) continue;
      const k = (e.expenseDate || '').substring(0, 7);
      exp.set(k, (exp.get(k) ?? 0) + Number(e.amount));
    }
    return this.monthKeys().map((k) => ({ key: k, label: this.label(k), income: 0, expense: exp.get(k) ?? 0 }));
  });

  private maxCatMonthly = computed(() => Math.max(1, ...this.catMonthly().map((m) => m.expense)));
  catTopMonth = computed(() => {
    const top = this.catMonthly().reduce<MonthPoint | null>((best, m) => (m.expense > (best?.expense ?? -1) ? m : best), null);
    return top && top.expense > 0 ? top : null;
  });

  topCategories = computed<CatBar[]>(() => {
    const byRoot = new Map<number, number>();
    const index = this.catIndex();
    for (const e of this.expenses()) {
      const root = this.rootOf(e.categoryId, index);
      byRoot.set(root, (byRoot.get(root) ?? 0) + Number(e.amount));
    }
    const entries = [...byRoot.entries()].sort((a, b) => b[1] - a[1]).slice(0, 8);
    const max = Math.max(1, ...entries.map(([, v]) => v));
    return entries.map(([id, amount]) => ({ name: index.get(id)?.name ?? `Categoría ${id}`, amount, pct: (amount / max) * 100 }));
  });

  ngOnInit(): void {
    this.finance.getIncomes().subscribe({ next: (r) => this.incomes.set(r) });
    this.finance.getExpenses().subscribe({ next: (r) => this.expenses.set(r) });
    this.finance.getExpenseCategories().subscribe({ next: (r) => this.cats.set(r) });
  }

  barPct(v: number): number {
    return (v / this.maxMonthly()) * 100;
  }
  catBarPct(v: number): number {
    return (v / this.maxCatMonthly()) * 100;
  }

  // --- Presets de rango ---

  applyPreset(months: number): void {
    this.fromMonth.set(this.monthsAgo(months - 1));
    this.toMonth.set(this.thisMonth());
  }

  applyYear(): void {
    this.fromMonth.set(`${new Date().getFullYear()}-01`);
    this.toMonth.set(this.thisMonth());
  }

  /** Desde el movimiento más antiguo (ingreso o gasto) hasta el mes actual. */
  applyAll(): void {
    const months = [
      ...this.incomes().map((i) => (i.incomeDate || '').substring(0, 7)),
      ...this.expenses().map((e) => (e.expenseDate || '').substring(0, 7)),
    ].filter(Boolean).sort();
    this.fromMonth.set(months[0] ?? this.monthsAgo(11));
    this.toMonth.set(this.thisMonth());
  }

  private thisMonth(): string {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
  }

  private monthsAgo(n: number): string {
    const d = new Date();
    const t = new Date(d.getFullYear(), d.getMonth() - n, 1);
    return `${t.getFullYear()}-${String(t.getMonth() + 1).padStart(2, '0')}`;
  }

  private label(key: string): string {
    const [y, m] = key.split('-');
    return `${MONTHS_ES[Number(m) - 1]} ${y.substring(2)}`;
  }

  private matchesCat(catId: number): boolean {
    const target = this.catId();
    if (target == null) return true;
    const index = this.catIndex();
    let cur: number | null | undefined = catId;
    const seen = new Set<number>();
    while (cur != null && !seen.has(cur)) {
      if (cur === target) return true;
      seen.add(cur);
      cur = index.get(cur)?.parentId ?? null;
    }
    return false;
  }

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
}
