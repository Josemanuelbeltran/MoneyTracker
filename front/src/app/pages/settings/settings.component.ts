import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { FinanceService } from '../../core/services/finance.service';
import { Account } from '../../core/models/account.model';
import { AccountType } from '../../core/models/account-type.model';
import { Category } from '../../core/models/category.model';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-bm-text">Ajustes</h1>
      <p class="text-sm text-bm-muted">Gestiona tus cuentas y categorías</p>
    </div>

    <div class="grid grid-cols-1 gap-5 xl:grid-cols-3">
      <!-- ===== Cuentas ===== -->
      <div class="mt-card overflow-hidden">
        <div class="border-b border-bm-border px-5 py-4">
          <h2 class="text-xs font-semibold uppercase tracking-wide text-bm-muted">Cuentas</h2>
        </div>
        <div>
          <div *ngFor="let a of accounts()" class="border-b border-bm-border px-5 py-3 last:border-0">
            <!-- edición -->
            <div *ngIf="editAccId() === a.id; else accView" class="space-y-2">
              <input class="mt-input" type="text" [(ngModel)]="eaName" />
              <select class="mt-input" [(ngModel)]="eaTypeId">
                <option *ngFor="let t of accountTypes()" [ngValue]="t.id">{{ t.name }}</option>
              </select>
              <input class="mt-input" type="number" [(ngModel)]="eaBalance" />
              <div class="flex gap-2">
                <button class="mt-btn-cyan flex-1" (click)="saveAccount()">Guardar</button>
                <button class="mt-btn-ghost flex-1" (click)="editAccId.set(null)">Cancelar</button>
              </div>
            </div>
            <ng-template #accView>
              <div class="flex items-center justify-between">
                <div>
                  <div class="text-sm font-medium text-bm-text">{{ a.name }}</div>
                  <div class="text-xs text-bm-muted">{{ typeName(a.accountTypeId) }} · {{ a.balance | number: '1.2-2' }} €</div>
                </div>
                <div class="flex items-center gap-2">
                  <button class="text-bm-muted hover:text-bm-cyan" (click)="startEditAccount(a)"><i class="pi pi-pencil"></i></button>
                  <button class="text-bm-muted hover:text-bm-danger" (click)="deleteAccount(a)"><i class="pi pi-trash"></i></button>
                </div>
              </div>
            </ng-template>
          </div>
          <div *ngIf="accounts().length === 0" class="px-5 py-6 text-center text-sm text-bm-muted">Sin cuentas</div>
        </div>
        <div class="space-y-2 border-t border-bm-border p-4">
          <input class="mt-input" type="text" placeholder="Nombre (ej. BBVA)" [(ngModel)]="accName" />
          <select class="mt-input" [(ngModel)]="accTypeId">
            <option [ngValue]="null" disabled>Tipo de cuenta</option>
            <option *ngFor="let t of accountTypes()" [ngValue]="t.id">{{ t.name }}</option>
          </select>
          <input class="mt-input" type="number" placeholder="Saldo inicial" [(ngModel)]="accBalance" />
          <button class="mt-btn-cyan w-full" (click)="addAccount()"><i class="pi pi-plus"></i> Añadir cuenta</button>
        </div>
      </div>

      <!-- ===== Categorías de ingreso ===== -->
      <div class="mt-card overflow-hidden">
        <div class="border-b border-bm-border px-5 py-4">
          <h2 class="text-xs font-semibold uppercase tracking-wide text-bm-muted">Categorías de ingreso</h2>
        </div>
        <div>
          <div *ngFor="let c of incomeCats()" class="flex items-center justify-between border-b border-bm-border px-5 py-3 last:border-0">
            <ng-container *ngIf="editIncId() === c.id; else incView">
              <div class="flex-1 space-y-2">
                <input class="mt-input" type="text" [(ngModel)]="eiName" />
                <label *ngIf="isAdmin" class="flex items-center gap-2 text-xs text-bm-muted">
                  <input type="checkbox" [(ngModel)]="eiCommon" /> Categoría común (para todos)
                </label>
              </div>
              <button class="mt-btn-cyan ml-2 mr-1 px-3" (click)="saveInc()">OK</button>
              <button class="mt-btn-ghost px-3" (click)="editIncId.set(null)">✕</button>
            </ng-container>
            <ng-template #incView>
              <span class="text-sm text-bm-text">
                {{ c.label || c.name }}
                <span *ngIf="c.isCommon" class="ml-2 rounded bg-bm-border px-1.5 py-0.5 text-[10px] uppercase tracking-wide text-bm-muted">Común</span>
              </span>
              <div class="flex items-center gap-2">
                <ng-container *ngIf="canEdit(c); else incLocked">
                  <button class="text-bm-muted hover:text-bm-cyan" (click)="startEditInc(c)"><i class="pi pi-pencil"></i></button>
                  <button class="text-bm-muted hover:text-bm-danger" (click)="deleteInc(c)"><i class="pi pi-trash"></i></button>
                </ng-container>
                <ng-template #incLocked>
                  <i class="pi pi-lock text-bm-muted" title="Categoría común: solo la puede cambiar un administrador"></i>
                </ng-template>
              </div>
            </ng-template>
          </div>
          <div *ngIf="incomeCats().length === 0" class="px-5 py-6 text-center text-sm text-bm-muted">Sin categorías</div>
        </div>
        <div class="space-y-2 border-t border-bm-border p-4">
          <input class="mt-input" type="text" placeholder="Nombre (ej. Nómina)" [(ngModel)]="incName" />
          <select class="mt-input" [(ngModel)]="incParentId">
            <option [ngValue]="null">Sin categoría padre</option>
            <option *ngFor="let c of incomeCats()" [ngValue]="c.id">{{ c.label || c.name }}</option>
          </select>
          <label *ngIf="isAdmin" class="flex items-center gap-2 text-xs text-bm-muted">
            <input type="checkbox" [(ngModel)]="incCommon" /> Categoría común (para todos)
          </label>
          <button class="mt-btn-cyan w-full" (click)="addIncomeCat()"><i class="pi pi-plus"></i> Añadir categoría</button>
        </div>
      </div>

      <!-- ===== Categorías de gasto ===== -->
      <div class="mt-card overflow-hidden">
        <div class="border-b border-bm-border px-5 py-4">
          <h2 class="text-xs font-semibold uppercase tracking-wide text-bm-muted">Categorías de gasto</h2>
        </div>
        <div>
          <div *ngFor="let c of expenseCats()" class="flex items-center justify-between border-b border-bm-border px-5 py-3 last:border-0">
            <ng-container *ngIf="editExpId() === c.id; else expView">
              <div class="flex-1 space-y-2">
                <input class="mt-input" type="text" [(ngModel)]="eeName" />
                <label *ngIf="isAdmin" class="flex items-center gap-2 text-xs text-bm-muted">
                  <input type="checkbox" [(ngModel)]="eeCommon" /> Categoría común (para todos)
                </label>
              </div>
              <button class="mt-btn-cyan ml-2 mr-1 px-3" (click)="saveExp()">OK</button>
              <button class="mt-btn-ghost px-3" (click)="editExpId.set(null)">✕</button>
            </ng-container>
            <ng-template #expView>
              <span class="text-sm text-bm-text">
                {{ c.label || c.name }}
                <span *ngIf="c.isCommon" class="ml-2 rounded bg-bm-border px-1.5 py-0.5 text-[10px] uppercase tracking-wide text-bm-muted">Común</span>
              </span>
              <div class="flex items-center gap-2">
                <ng-container *ngIf="canEdit(c); else expLocked">
                  <button class="text-bm-muted hover:text-bm-cyan" (click)="startEditExp(c)"><i class="pi pi-pencil"></i></button>
                  <button class="text-bm-muted hover:text-bm-danger" (click)="deleteExp(c)"><i class="pi pi-trash"></i></button>
                </ng-container>
                <ng-template #expLocked>
                  <i class="pi pi-lock text-bm-muted" title="Categoría común: solo la puede cambiar un administrador"></i>
                </ng-template>
              </div>
            </ng-template>
          </div>
          <div *ngIf="expenseCats().length === 0" class="px-5 py-6 text-center text-sm text-bm-muted">Sin categorías</div>
        </div>
        <div class="space-y-2 border-t border-bm-border p-4">
          <input class="mt-input" type="text" placeholder="Nombre (ej. Alquiler)" [(ngModel)]="expName" />
          <select class="mt-input" [(ngModel)]="expParentId">
            <option [ngValue]="null">Sin categoría padre</option>
            <option *ngFor="let c of expenseCats()" [ngValue]="c.id">{{ c.label || c.name }}</option>
          </select>
          <label *ngIf="isAdmin" class="flex items-center gap-2 text-xs text-bm-muted">
            <input type="checkbox" [(ngModel)]="expCommon" /> Categoría común (para todos)
          </label>
          <button class="mt-btn-cyan w-full" (click)="addExpenseCat()"><i class="pi pi-plus"></i> Añadir categoría</button>
        </div>
      </div>
    </div>
  `,
})
export class SettingsComponent implements OnInit {
  private finance = inject(FinanceService);
  private msg = inject(MessageService);
  private auth = inject(AuthService);

  /** Solo el admin decide qué categorías son comunes (el backend lo revalida). */
  readonly isAdmin = this.auth.isAdmin();

  accounts = signal<Account[]>([]);
  accountTypes = signal<AccountType[]>([]);
  incomeCats = signal<Category[]>([]);
  expenseCats = signal<Category[]>([]);

  // formularios de alta
  accName = '';
  accTypeId: number | null = null;
  accBalance: number | null = null;
  incName = '';
  incParentId: number | null = null;
  incCommon = false;
  expName = '';
  expParentId: number | null = null;
  expCommon = false;

  // edición en línea
  editAccId = signal<number | null>(null);
  eaName = '';
  eaTypeId: number | null = null;
  eaBalance: number | null = null;

  editIncId = signal<number | null>(null);
  eiName = '';
  eiCommon = false;

  editExpId = signal<number | null>(null);
  eeName = '';
  eeParentId: number | null = null;
  eeCommon = false;

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.finance.getAccounts().subscribe({ next: (r) => this.accounts.set(r) });
    this.finance.getAccountTypes().subscribe({ next: (r) => this.accountTypes.set(r) });
    this.finance.getIncomeCategories().subscribe({ next: (r) => this.incomeCats.set(r) });
    this.finance.getExpenseCategories().subscribe({ next: (r) => this.expenseCats.set(r) });
  }

  typeName(id: number): string {
    return this.accountTypes().find((t) => t.id === id)?.name ?? '';
  }

  /** Las comunes son de solo lectura salvo para el admin; las propias siempre se editan. */
  canEdit(c: Category): boolean {
    return !c.isCommon || this.isAdmin;
  }

  private ok(detail: string): void {
    this.msg.add({ severity: 'success', summary: detail, detail: '' });
  }
  private fail(detail: string): void {
    this.msg.add({ severity: 'error', summary: 'Error', detail });
  }
  private failFrom(e: unknown, fallback: string): void {
    const detail = (e as { error?: { error?: string } })?.error?.error ?? fallback;
    this.msg.add({ severity: 'error', summary: 'No se pudo borrar', detail });
  }
  /** Usa el mensaje del backend (p.ej. "las comunes solo las edita un admin") si viene. */
  private failFromError(e: unknown, fallback: string): void {
    const detail = (e as { error?: { error?: string; message?: string } })?.error?.error
      ?? (e as { error?: { message?: string } })?.error?.message
      ?? fallback;
    this.msg.add({ severity: 'error', summary: 'Error', detail });
  }

  // ---- Cuentas ----
  addAccount(): void {
    if (!this.accName || !this.accTypeId) {
      this.msg.add({ severity: 'warn', summary: 'Faltan datos', detail: 'Nombre y tipo de cuenta' });
      return;
    }
    this.finance.createAccount({ name: this.accName, accountTypeId: this.accTypeId, balance: this.accBalance || 0 }).subscribe({
      next: () => { this.ok('Cuenta creada'); this.accName = ''; this.accTypeId = null; this.accBalance = null; this.reload(); },
      error: () => this.fail('No se pudo crear la cuenta'),
    });
  }
  startEditAccount(a: Account): void {
    this.editAccId.set(a.id);
    this.eaName = a.name;
    this.eaTypeId = a.accountTypeId;
    this.eaBalance = a.balance;
  }
  saveAccount(): void {
    const id = this.editAccId();
    if (!id || !this.eaName || !this.eaTypeId) return;
    this.finance.updateAccount(id, { name: this.eaName, accountTypeId: this.eaTypeId, balance: this.eaBalance || 0 }).subscribe({
      next: () => { this.ok('Cuenta actualizada'); this.editAccId.set(null); this.reload(); },
      error: () => this.fail('No se pudo actualizar'),
    });
  }
  deleteAccount(a: Account): void {
    if (!confirm(`¿Borrar la cuenta "${a.name}"?`)) return;
    this.finance.deleteAccount(a.id).subscribe({
      next: () => { this.ok('Cuenta borrada'); this.reload(); },
      error: (e) => this.failFrom(e, 'No se pudo borrar la cuenta'),
    });
  }

  // ---- Categorías de ingreso ----
  addIncomeCat(): void {
    if (!this.incName) return;
    this.finance.createIncomeCategory(this.incName, this.incParentId, this.incCommon).subscribe({
      next: () => { this.ok('Categoría creada'); this.incName = ''; this.incParentId = null; this.incCommon = false; this.reload(); },
      error: (e) => this.failFromError(e, 'No se pudo crear'),
    });
  }
  startEditInc(c: Category): void { this.editIncId.set(c.id); this.eiName = c.name; this.eiCommon = !!c.isCommon; }
  saveInc(): void {
    const id = this.editIncId();
    if (!id || !this.eiName) return;
    // Solo el admin manda isCommon; para el resto el backend ignora el campo.
    this.finance.updateIncomeCategory(id, this.eiName, this.isAdmin ? this.eiCommon : undefined).subscribe({
      next: () => { this.ok('Categoría actualizada'); this.editIncId.set(null); this.reload(); },
      error: (e) => this.failFromError(e, 'No se pudo actualizar'),
    });
  }
  deleteInc(c: Category): void {
    if (!confirm(`¿Borrar la categoría "${c.name}"?`)) return;
    this.finance.deleteIncomeCategory(c.id).subscribe({
      next: () => { this.ok('Categoría borrada'); this.reload(); },
      error: (e) => this.failFromError(e, 'No se pudo borrar la categoría'),
    });
  }

  // ---- Categorías de gasto ----
  addExpenseCat(): void {
    if (!this.expName) return;
    this.finance.createExpenseCategory(this.expName, this.expParentId, this.expCommon).subscribe({
      next: () => { this.ok('Categoría creada'); this.expName = ''; this.expParentId = null; this.expCommon = false; this.reload(); },
      error: (e) => this.failFromError(e, 'No se pudo crear'),
    });
  }
  startEditExp(c: Category): void {
    this.editExpId.set(c.id);
    this.eeName = c.name;
    this.eeParentId = c.parentId ?? null;
    this.eeCommon = !!c.isCommon;
  }
  saveExp(): void {
    const id = this.editExpId();
    if (!id || !this.eeName) return;
    // Solo el admin manda isCommon; para el resto el backend ignora el campo.
    this.finance.updateExpenseCategory(id, this.eeName, this.eeParentId, this.isAdmin ? this.eeCommon : undefined).subscribe({
      next: () => { this.ok('Categoría actualizada'); this.editExpId.set(null); this.reload(); },
      error: (e) => this.failFromError(e, 'No se pudo actualizar'),
    });
  }
  deleteExp(c: Category): void {
    if (!confirm(`¿Borrar la categoría "${c.name}"?`)) return;
    this.finance.deleteExpenseCategory(c.id).subscribe({
      next: () => { this.ok('Categoría borrada'); this.reload(); },
      error: (e) => this.failFromError(e, 'No se pudo borrar la categoría'),
    });
  }
}
