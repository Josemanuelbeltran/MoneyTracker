import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { FinanceService } from '../../core/services/finance.service';
import { Income } from '../../core/models/movement.model';
import { Account } from '../../core/models/account.model';
import { Category } from '../../core/models/category.model';
import { CalcAmountComponent } from '../../shared/calc-amount.component';
import { CategoryPickerComponent } from '../../shared/category-picker.component';

interface Form {
  accountId: number | null;
  categoryId: number | null;
  amount: number | null;
  description: string;
  date: string;
  isRecurring: boolean;
  monthsAhead: number;
}

/** Acción pendiente de que el usuario elija el alcance (solo este / este y siguientes). */
interface ScopeAsk {
  action: 'edit' | 'delete';
  income: Income;
}

interface AccountGroup {
  accountId: number;
  name: string;
  subtotal: number;
  items: Income[];
}

type PeriodMode = 'month' | 'year' | 'all';

@Component({
  selector: 'app-incomes',
  standalone: true,
  imports: [CommonModule, FormsModule, CalcAmountComponent, CategoryPickerComponent],
  template: `
    <div class="mb-6 flex flex-wrap items-center justify-between gap-3">
      <div>
        <h1 class="text-2xl font-bold text-bm-text">Ingresos</h1>
        <p class="text-sm text-bm-muted">
          {{ periodLabel() }}: <span class="font-semibold text-bm-success">{{ total() | number: '1.2-2' }} €</span>
        </p>
      </div>
      <button class="mt-btn-cyan" (click)="openAdd()"><i class="pi pi-plus"></i> Añadir ingreso</button>
    </div>

    <!-- Selector de periodo -->
    <div class="mb-5 flex flex-wrap items-center gap-3">
      <div class="inline-flex overflow-hidden rounded-lg border border-bm-border">
        <button *ngFor="let m of modes"
                (click)="periodMode.set(m.key)"
                class="px-3 py-1.5 text-sm transition-colors"
                [class.bg-bm-surface-2]="periodMode() === m.key"
                [class.text-bm-cyan]="periodMode() === m.key"
                [class.text-bm-muted]="periodMode() !== m.key">
          {{ m.label }}
        </button>
      </div>
      <div *ngIf="periodMode() === 'month'" class="flex items-center gap-2">
        <button class="mt-btn-ghost" (click)="shiftMonth(-1)"><i class="pi pi-chevron-left"></i></button>
        <input class="mt-input w-40" type="month" [ngModel]="month()" (ngModelChange)="month.set($event)" />
        <button class="mt-btn-ghost" (click)="shiftMonth(1)"><i class="pi pi-chevron-right"></i></button>
      </div>
      <div *ngIf="periodMode() === 'year'" class="flex items-center gap-2">
        <button class="mt-btn-ghost" (click)="year.set(year() - 1)"><i class="pi pi-chevron-left"></i></button>
        <span class="w-14 text-center text-sm font-semibold text-bm-text">{{ year() }}</span>
        <button class="mt-btn-ghost" (click)="year.set(year() + 1)"><i class="pi pi-chevron-right"></i></button>
      </div>

      <select class="mt-input sm:ml-auto sm:w-56" [ngModel]="catFilter()" (ngModelChange)="catFilter.set($event)">
        <option [ngValue]="null">Todas las categorías</option>
        <option *ngFor="let c of categories()" [ngValue]="c.id">{{ c.label || c.name }}</option>
      </select>
    </div>

    <!-- Grupos por cuenta -->
    <div class="space-y-4">
      <div *ngFor="let g of groups()" class="mt-card overflow-hidden">
        <div class="flex items-center justify-between border-b border-bm-border px-5 py-3">
          <div class="flex items-center gap-2">
            <i class="pi pi-wallet text-bm-cyan"></i>
            <span class="text-sm font-semibold text-bm-text">{{ g.name }}</span>
          </div>
          <span class="text-sm font-semibold text-bm-success">+{{ g.subtotal | number: '1.2-2' }} €</span>
        </div>
        <div *ngFor="let i of g.items" class="flex items-center gap-3 border-b border-bm-border px-5 py-3 last:border-0"
             [class.opacity-70]="isFuture(i.incomeDate)">
          <div class="flex-1 overflow-hidden">
            <div class="flex items-center gap-2">
              <span class="truncate text-sm font-medium text-bm-text">{{ i.description || '(sin descripción)' }}</span>
              <span *ngIf="i.isRecurring" class="shrink-0 rounded bg-bm-surface-2 px-1.5 py-0.5 text-[10px] font-semibold text-bm-cyan">🔁 Fijo</span>
              <span *ngIf="isFuture(i.incomeDate)" class="shrink-0 rounded bg-bm-surface-2 px-1.5 py-0.5 text-[10px] font-semibold text-bm-muted">⏳ Futuro</span>
            </div>
            <div class="text-xs text-bm-muted">{{ i.incomeDate | slice: 0:10 }} · {{ categoryName(i.categoryId) }}</div>
          </div>
          <div class="text-sm font-semibold text-bm-success">+{{ i.amount | number: '1.2-2' }} €</div>
          <button class="text-bm-muted transition-colors hover:text-bm-cyan" (click)="openEdit(i)">
            <i class="pi pi-pencil"></i>
          </button>
          <button class="text-bm-muted transition-colors hover:text-bm-danger" (click)="remove(i)">
            <i class="pi pi-trash"></i>
          </button>
        </div>
      </div>

      <div *ngIf="groups().length === 0" class="mt-card px-5 py-10 text-center text-sm text-bm-muted">
        No hay ingresos en este periodo.
      </div>
    </div>

    <!-- Modal -->
    <div *ngIf="showAdd()" class="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/60 p-4"
         (click)="showAdd.set(false)">
      <div class="mt-card w-full max-w-md p-6 sm:my-auto" (click)="$event.stopPropagation()">
        <div class="mb-4 flex items-center justify-between">
          <h2 class="text-lg font-semibold text-bm-text">{{ editingId() ? 'Editar ingreso' : 'Nuevo ingreso' }}</h2>
          <button class="text-bm-muted hover:text-bm-text" (click)="showAdd.set(false)"><i class="pi pi-times"></i></button>
        </div>

        <label class="mt-label">Importe (€)</label>
        <div class="mb-3"><app-calc-amount [(value)]="form.amount" /></div>
        <label class="mt-label">Categoría</label>
        <div class="mb-3"><app-category-picker [categories]="categories()" [(value)]="form.categoryId" /></div>
        <label class="mt-label">Cuenta</label>
        <select class="mt-input mb-3" [(ngModel)]="form.accountId">
          <option [ngValue]="null" disabled>Selecciona cuenta</option>
          <option *ngFor="let a of accounts()" [ngValue]="a.id">{{ a.name }}</option>
        </select>
        <label class="mt-label">Fecha</label>
        <input class="mt-input mb-3" type="date" [(ngModel)]="form.date" />
        <label class="mt-label">Descripción</label>
        <input class="mt-input mb-4" type="text" [(ngModel)]="form.description" />

        <label class="mb-3 flex cursor-pointer items-center gap-2 text-sm text-bm-text">
          <input type="checkbox" class="h-4 w-4 accent-bm-cyan" [(ngModel)]="form.isRecurring" />
          🔁 Fijo (se repite cada mes)
        </label>

        <!-- Al crear un fijo se materializan N meses por adelantado -->
        <div *ngIf="form.isRecurring && !editingId()" class="mb-5 rounded-lg border border-bm-border bg-bm-surface-2 p-3">
          <label class="mt-label">Meses a generar por adelantado</label>
          <input class="mt-input" type="number" min="1" max="60" [(ngModel)]="form.monthsAhead" />
          <p class="mt-2 text-xs text-bm-muted">
            Se crearán {{ form.monthsAhead }} ingresos, uno cada mes desde la fecha indicada.
          </p>
        </div>
        <div *ngIf="!form.isRecurring || editingId()" class="mb-2"></div>

        <button class="mt-btn-cyan w-full" (click)="save()"><i class="pi pi-save"></i> {{ editingId() ? 'Guardar cambios' : 'Guardar ingreso' }}</button>
      </div>
    </div>

    <!-- Modal de alcance: solo este / este y siguientes -->
    <div *ngIf="scopeAsk() as ask" class="fixed inset-0 z-[60] flex items-center justify-center bg-black/60 p-4"
         (click)="scopeAsk.set(null)">
      <div class="mt-card w-full max-w-sm p-6" (click)="$event.stopPropagation()">
        <h2 class="mb-1 text-lg font-semibold text-bm-text">
          {{ ask.action === 'edit' ? 'Editar ingreso fijo' : 'Borrar ingreso fijo' }}
        </h2>
        <p class="mb-5 text-sm text-bm-muted">
          Este ingreso es fijo. ¿Qué quieres {{ ask.action === 'edit' ? 'modificar' : 'borrar' }}?
          Los anteriores a {{ ask.income.incomeDate | slice: 0:10 }} no se tocan.
        </p>

        <button class="mt-btn-ghost mb-2 w-full justify-center" (click)="applyScope('one')">
          Solo este
        </button>
        <button class="w-full justify-center"
                [class.mt-btn-cyan]="ask.action === 'edit'"
                [class.mt-btn-danger]="ask.action === 'delete'"
                (click)="applyScope('forward')">
          Este y los siguientes
        </button>
      </div>
    </div>
  `,
})
export class IncomesComponent implements OnInit {
  private finance = inject(FinanceService);
  private msg = inject(MessageService);

  items = signal<Income[]>([]);
  accounts = signal<Account[]>([]);
  categories = signal<Category[]>([]);
  showAdd = signal(false);
  editingId = signal<number | null>(null);
  scopeAsk = signal<ScopeAsk | null>(null);
  form: Form = this.empty();
  /** Ingreso original que se está editando (para saber si ya era fijo). */
  private editing: Income | null = null;

  readonly modes: { key: PeriodMode; label: string }[] = [
    { key: 'month', label: 'Mes' },
    { key: 'year', label: 'Año' },
    { key: 'all', label: 'Todo' },
  ];
  periodMode = signal<PeriodMode>('month');
  month = signal<string>(new Date().toISOString().substring(0, 7));
  year = signal<number>(new Date().getFullYear());
  catFilter = signal<number | null>(null);

  private catIndex = computed(() => new Map(this.categories().map((c) => [c.id, c])));

  private filtered = computed(() =>
    this.items().filter((i) => this.inPeriod(i.incomeDate) && this.matchesCat(i.categoryId)),
  );
  total = computed(() => this.filtered().reduce((s, i) => s + Number(i.amount), 0));

  groups = computed<AccountGroup[]>(() => {
    const byAcc = new Map<number, Income[]>();
    for (const i of this.filtered()) {
      const arr = byAcc.get(i.accountId) ?? [];
      arr.push(i);
      byAcc.set(i.accountId, arr);
    }
    return [...byAcc.entries()]
      .map(([accountId, list]) => ({
        accountId,
        name: this.accountName(accountId),
        subtotal: list.reduce((s, i) => s + Number(i.amount), 0),
        items: list.sort((a, b) => (b.incomeDate || '').localeCompare(a.incomeDate || '')),
      }))
      .sort((a, b) => b.subtotal - a.subtotal);
  });

  periodLabel = computed(() => {
    switch (this.periodMode()) {
      case 'month':
        return 'Mes';
      case 'year':
        return 'Año ' + this.year();
      default:
        return 'Total';
    }
  });

  ngOnInit(): void {
    this.load();
    this.finance.getAccounts().subscribe({ next: (r) => this.accounts.set(r) });
    this.finance.getIncomeCategories().subscribe({ next: (r) => this.categories.set(r) });
  }

  load(): void {
    this.finance.getIncomes().subscribe({ next: (r) => this.items.set(r) });
  }

  accountName(id: number): string {
    return this.accounts().find((a) => a.id === id)?.name ?? `cuenta ${id}`;
  }

  categoryName(id: number): string {
    return this.categories().find((c) => c.id === id)?.name ?? `cat ${id}`;
  }

  openAdd(): void {
    this.editingId.set(null);
    this.editing = null;
    this.form = this.empty();
    this.showAdd.set(true);
  }

  openEdit(income: Income): void {
    this.editingId.set(income.id);
    this.form = {
      accountId: income.accountId,
      categoryId: income.categoryId,
      amount: Number(income.amount),
      description: income.description ?? '',
      date: (income.incomeDate || '').substring(0, 10),
      isRecurring: income.isRecurring ?? false,
      monthsAhead: 12,
    };
    this.editing = income;
    this.showAdd.set(true);
  }

  save(): void {
    const f = this.form;
    if (!f.accountId || !f.categoryId || !f.amount) {
      this.msg.add({ severity: 'warn', summary: 'Faltan datos', detail: 'Rellena cuenta, categoría e importe' });
      return;
    }
    const id = this.editingId();

    // Editar un ingreso que ya era fijo: preguntar el alcance antes de tocar nada.
    if (id && this.editing?.isRecurring) {
      this.scopeAsk.set({ action: 'edit', income: this.editing });
      return;
    }

    const body: Partial<Income> = {
      accountId: f.accountId,
      categoryId: f.categoryId,
      amount: f.amount,
      description: f.description,
      incomeDate: f.date,
      isRecurring: f.isRecurring,
    };

    // Alta de un fijo nuevo: se materializan N meses de golpe.
    if (!id && f.isRecurring) {
      this.finance.createRecurringIncome(body, f.monthsAhead).subscribe({
        next: (r) => {
          this.showAdd.set(false);
          this.msg.add({ severity: 'success', summary: 'Ingreso fijo creado', detail: `${r.created} meses generados` });
          this.load();
        },
        error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo crear el ingreso fijo' }),
      });
      return;
    }

    const req$ = id ? this.finance.updateIncome(id, body) : this.finance.createIncome(body);
    req$.subscribe({
      next: () => {
        this.showAdd.set(false);
        this.msg.add({ severity: 'success', summary: id ? 'Ingreso actualizado' : 'Ingreso añadido', detail: '' });
        this.load();
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: '¿Ids de cuenta/categoría válidos?' }),
    });
  }

  remove(income: Income): void {
    // Los fijos preguntan alcance; los sueltos se borran directamente (como hasta ahora).
    if (income.isRecurring) {
      this.scopeAsk.set({ action: 'delete', income });
      return;
    }
    this.deleteOne(income.id);
  }

  /** Resuelve el modal de alcance: 'one' = solo este, 'forward' = este y los siguientes. */
  applyScope(scope: 'one' | 'forward'): void {
    const ask = this.scopeAsk();
    if (!ask) return;
    this.scopeAsk.set(null);

    if (ask.action === 'delete') {
      if (scope === 'one') {
        this.deleteOne(ask.income.id);
        return;
      }
      this.finance.deleteIncomeSeries(ask.income.id).subscribe({
        next: (r) => {
          this.msg.add({ severity: 'info', summary: 'Serie eliminada', detail: `${r.deleted} ingresos borrados` });
          this.load();
        },
        error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo eliminar la serie' }),
      });
      return;
    }

    const f = this.form;
    if (scope === 'forward') {
      // Hacia delante solo se propaga el importe (subida de nómina, etc.).
      this.finance.updateIncomeSeries(ask.income.id, f.amount!).subscribe({
        next: (r) => {
          this.showAdd.set(false);
          this.msg.add({ severity: 'success', summary: 'Serie actualizada', detail: `${r.updated} ingresos con el nuevo importe` });
          this.load();
        },
        error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo actualizar la serie' }),
      });
      return;
    }

    this.finance
      .updateIncome(ask.income.id, {
        accountId: f.accountId!,
        categoryId: f.categoryId!,
        amount: f.amount!,
        description: f.description,
        incomeDate: f.date,
        isRecurring: f.isRecurring,
      })
      .subscribe({
        next: () => {
          this.showAdd.set(false);
          this.msg.add({ severity: 'success', summary: 'Ingreso actualizado', detail: '' });
          this.load();
        },
        error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo actualizar' }),
      });
  }

  private deleteOne(id: number): void {
    this.finance.deleteIncome(id).subscribe({
      next: () => {
        this.msg.add({ severity: 'info', summary: 'Ingreso eliminado', detail: '' });
        this.load();
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo eliminar' }),
    });
  }

  /** Avanza/retrocede el mes seleccionado (formato 'YYYY-MM'). */
  shiftMonth(delta: number): void {
    const [y, m] = this.month().split('-').map(Number);
    const d = new Date(y, m - 1 + delta, 1);
    this.month.set(`${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`);
  }

  /** ¿La fecha del ingreso es posterior a hoy? */
  isFuture(date: string): boolean {
    return (date || '').substring(0, 10) > new Date().toISOString().substring(0, 10);
  }

  /** ¿La categoría del movimiento coincide con el filtro (o desciende de él)? */
  private matchesCat(catId: number): boolean {
    const target = this.catFilter();
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

  private inPeriod(date: string): boolean {
    const d = (date || '').substring(0, 10);
    switch (this.periodMode()) {
      case 'all':
        return true;
      case 'month':
        return d.substring(0, 7) === this.month();
      case 'year':
        return d.substring(0, 4) === String(this.year());
    }
  }

  private empty(): Form {
    return {
      accountId: null,
      categoryId: null,
      amount: null,
      description: '',
      date: new Date().toISOString().substring(0, 10),
      isRecurring: false,
      monthsAhead: 12,
    };
  }
}
