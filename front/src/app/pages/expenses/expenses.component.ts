import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { FinanceService } from '../../core/services/finance.service';
import { Expense, Reimbursement } from '../../core/models/movement.model';
import { Account } from '../../core/models/account.model';
import { Category } from '../../core/models/category.model';
import { Pocket } from '../../core/models/pocket.model';
import { Pet } from '../../core/models/pet.model';
import { Person } from '../../core/models/person.model';
import { environment } from '../../../environments/environment';
import { CalcAmountComponent } from '../../shared/calc-amount.component';
import { CategoryPickerComponent } from '../../shared/category-picker.component';

interface Form {
  accountId: number | null;
  categoryId: number | null;
  amount: number | null;
  description: string;
  date: string;
  isRecurring: boolean;
  pocketId: number | null;
  /** Mascota a la que va el gasto (id de PetRecording). Solo en la categoria de mascotas. */
  petId: number | null;
  /** Alta de mascota sin salir del modal: el id es el de PetRecording. */
  newPetId: number | null;
  newPetName: string;
  isSplit: boolean;
  /** 'equal' = a partes iguales; 'advance' = reparto libre y lo adelantas tú. */
  splitMode: SplitMode;
  splitParts: number;
  /** Tu parte en el reparto libre, en % o en € según `myShareUnit`. */
  myShareInput: number | null;
  myShareUnit: 'pct' | 'eur';
  /** A partes iguales: además de dividir, pagas tú el total y te lo devuelven. */
  equalAdvance: boolean;
  /** Quién te debe cada parte que no es tuya. Tantas como splitParts − 1. */
  debtorPersonIds: (number | null)[];
  /** Cuenta donde te devuelven lo que adelantaste. null = no te lo devuelven. */
  reimburseToAccountId: number | null;
  /** Persona seleccionada; NEW_PERSON abre el campo para crear una nueva. */
  personId: number | null;
  newPersonName: string;
  isSpread: boolean;
  spreadMonths: number;
  /** Reembolso ya existente, al editar un gasto que lo tiene. null si no hay. */
  reimbId: number | null;
  reimbAmount: number | null;
}

type SplitMode = 'equal' | 'advance';

interface AccountGroup {
  accountId: number;
  name: string;
  subtotal: number;
  items: Expense[];
}

type PeriodMode = 'month' | 'year' | 'all';

@Component({
  selector: 'app-expenses',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, CalcAmountComponent, CategoryPickerComponent],
  template: `
    <div class="mb-6 flex flex-wrap items-center justify-between gap-3">
      <div>
        <h1 class="text-2xl font-bold text-bm-text">Gastos</h1>
        <p class="text-sm text-bm-muted">
          {{ periodLabel() }}: <span class="font-semibold text-bm-danger">{{ total() | number: '1.2-2' }} €</span>
        </p>
      </div>
      <button class="mt-btn-cyan" (click)="openAdd()"><i class="pi pi-plus"></i> Añadir gasto</button>
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
      <input *ngIf="periodMode() === 'month'" class="mt-input" type="month" [ngModel]="month()" (ngModelChange)="month.set($event)" />
      <div *ngIf="periodMode() === 'year'" class="flex items-center gap-2">
        <button class="mt-btn-ghost" (click)="year.set(year() - 1)"><i class="pi pi-chevron-left"></i></button>
        <span class="w-14 text-center text-sm font-semibold text-bm-text">{{ year() }}</span>
        <button class="mt-btn-ghost" (click)="year.set(year() + 1)"><i class="pi pi-chevron-right"></i></button>
      </div>

      <select class="mt-input sm:ml-auto sm:w-48" [ngModel]="accFilter()" (ngModelChange)="onAccFilterChange($event)">
        <option [ngValue]="null">Todas las cuentas</option>
        <option *ngFor="let a of accounts()" [ngValue]="a.id">{{ a.name }}</option>
      </select>

      <!-- Solo si hay sobres que filtrar; sin cuenta elegida salen los de todas -->
      <select *ngIf="filterPockets().length > 0" class="mt-input sm:w-48"
              [ngModel]="pocketFilter()" (ngModelChange)="pocketFilter.set($event)">
        <option [ngValue]="null">Todos los sobres</option>
        <option [ngValue]="'none'">Sin sobre</option>
        <option *ngFor="let p of filterPockets()" [ngValue]="p.id">{{ pocketFilterLabel(p) }}</option>
      </select>

      <select class="mt-input sm:w-56" [ngModel]="catFilter()" (ngModelChange)="catFilter.set($event)">
        <option [ngValue]="null">Todas las categorías</option>
        <option *ngFor="let c of categories()" [ngValue]="c.id">{{ c.label || c.name }}</option>
      </select>
    </div>

    <!-- Resumen de lo que te deben; el detalle por persona está en Cobros -->
    <a *ngIf="pendingReimbursements().length > 0" routerLink="/pending"
       class="mt-card mb-5 flex items-center gap-3 px-5 py-3 transition-colors hover:bg-bm-surface-2">
      <i class="pi pi-clock text-bm-warning"></i>
      <span class="flex-1 text-sm text-bm-text">
        Te deben <span class="font-semibold text-bm-warning">{{ pendingTotal() | number: '1.2-2' }} €</span>
        en {{ pendingReimbursements().length }}
        {{ pendingReimbursements().length === 1 ? 'cobro' : 'cobros' }}
      </span>
      <span class="text-xs text-bm-cyan">Ver cobros <i class="pi pi-chevron-right text-[10px]"></i></span>
    </a>

    <!-- Grupos por cuenta -->
    <div class="space-y-4">
      <div *ngFor="let g of groups()" class="mt-card overflow-hidden">
        <div class="flex items-center justify-between border-b border-bm-border px-5 py-3">
          <div class="flex items-center gap-2">
            <i class="pi pi-wallet text-bm-cyan"></i>
            <span class="text-sm font-semibold text-bm-text">{{ g.name }}</span>
          </div>
          <span class="text-sm font-semibold text-bm-danger">-{{ g.subtotal | number: '1.2-2' }} €</span>
        </div>
        <div *ngFor="let e of g.items" class="flex items-center gap-3 border-b border-bm-border px-5 py-3 last:border-0">
          <div class="flex-1 overflow-hidden">
            <div class="flex items-center gap-2">
              <span class="truncate text-sm font-medium text-bm-text">{{ e.description || '(sin descripción)' }}</span>
              <span *ngIf="e.isRecurring" class="shrink-0 rounded bg-bm-surface-2 px-1.5 py-0.5 text-[10px] font-semibold text-bm-cyan">🔁 Fijo</span>
              <span *ngIf="e.splitParts" class="shrink-0 rounded bg-bm-surface-2 px-1.5 py-0.5 text-[10px] font-semibold text-bm-muted">🧑‍🤝‍🧑 1/{{ e.splitParts }}</span>
              <span *ngIf="e.spreadCount" class="shrink-0 rounded bg-bm-surface-2 px-1.5 py-0.5 text-[10px] font-semibold text-bm-muted">📅 {{ e.spreadIndex }}/{{ e.spreadCount }}</span>
            </div>
            <div class="text-xs text-bm-muted">
              {{ e.expenseDate | slice: 0:10 }} · {{ categoryName(e.categoryId) }}<span *ngIf="e.pocketId"> · 📦 {{ pocketName(e.pocketId) }}</span><span *ngIf="e.petId"> · 🐾 {{ petName(e.petId) }}</span>
              <span *ngIf="e.totalAmount && e.splitParts"> · tu parte de {{ e.totalAmount | number: '1.2-2' }} € entre {{ e.splitParts }}</span>
              <span *ngIf="e.totalAmount && !e.splitParts"> · tu parte de {{ e.totalAmount | number: '1.2-2' }} €</span>
            </div>
            <!-- Adelantos: una línea por persona que te debe su parte -->
            <div *ngFor="let r of reimbByExpense().get(e.id) || []" class="mt-1 text-xs">
              <ng-container *ngIf="!r.settledAt">
                <span class="text-bm-warning">
                  ⏳ {{ r.debtor || 'Alguien' }} te debe {{ r.amount | number: '1.2-2' }} €
                  en {{ accountName(r.toAccountId) }}
                </span>
                <button class="ml-2 text-bm-cyan hover:underline" (click)="settle(r)">Cobrado</button>
              </ng-container>
              <ng-container *ngIf="r.settledAt">
                <span class="text-bm-success">
                  ✅ {{ r.debtor || 'Alguien' }} te pagó {{ r.amount | number: '1.2-2' }} €
                  en {{ accountName(r.toAccountId) }}
                </span>
                <button class="ml-2 text-bm-muted hover:underline" (click)="unsettle(r)">Deshacer</button>
              </ng-container>
            </div>
          </div>
          <div class="text-sm font-semibold text-bm-danger">-{{ e.amount | number: '1.2-2' }} €</div>
          <button class="text-bm-muted transition-colors hover:text-bm-cyan" (click)="openEdit(e)">
            <i class="pi pi-pencil"></i>
          </button>
          <button class="text-bm-muted transition-colors hover:text-bm-danger" (click)="remove(e)">
            <i class="pi pi-trash"></i>
          </button>
        </div>
      </div>

      <div *ngIf="groups().length === 0" class="mt-card px-5 py-10 text-center text-sm text-bm-muted">
        No hay gastos en este periodo.
      </div>
    </div>

    <!-- Modal -->
    <div *ngIf="showAdd()" class="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/60 p-4"
         (click)="showAdd.set(false)">
      <div class="mt-card w-full max-w-md p-6 sm:my-auto" (click)="$event.stopPropagation()">
        <div class="mb-4 flex items-center justify-between">
          <h2 class="text-lg font-semibold text-bm-text">{{ editingId() ? 'Editar gasto' : 'Nuevo gasto' }}</h2>
          <button class="text-bm-muted hover:text-bm-text" (click)="showAdd.set(false)"><i class="pi pi-times"></i></button>
        </div>

        <label class="mt-label">Importe (€)</label>
        <div class="mb-3"><app-calc-amount [(value)]="form.amount" /></div>
        <label class="mt-label">Categoría</label>
        <div class="mb-3"><app-category-picker [categories]="categories()" [(value)]="form.categoryId" /></div>

        <!-- Solo en Mascotas (o sus subcategorías): el gasto se manda a PetRecording -->
        <ng-container *ngIf="isPetCategory(form.categoryId)">
          <label class="mt-label">Mascota <span class="text-bm-muted">— se envía a PetRecording</span></label>
          <select class="mt-input mb-2" [(ngModel)]="form.petId">
            <option [ngValue]="null">Sin mascota</option>
            <option *ngFor="let p of pets()" [ngValue]="p.id">{{ p.name }}</option>
          </select>

          <!-- Dar de alta sin salir del modal, como las personas. Aquí además hay
               que poner el id, porque es el de PetRecording y no se puede inventar. -->
          <div class="flex gap-2">
            <input class="mt-input w-20" type="number" min="1" [(ngModel)]="form.newPetId"
                   placeholder="id" (keyup.enter)="addPet()" />
            <input class="mt-input flex-1" type="text" [(ngModel)]="form.newPetName"
                   placeholder="Añadir mascota nueva" (keyup.enter)="addPet()" />
            <button type="button" class="mt-btn-ghost text-xs" (click)="addPet()"
                    [disabled]="!form.newPetName.trim() || !form.newPetId">
              <i class="pi pi-plus"></i> Crear
            </button>
          </div>
          <p class="mb-3 mt-1 text-xs text-bm-muted">
            El id tiene que ser el mismo que tiene la mascota en PetRecording.
          </p>
        </ng-container>

        <label class="mt-label">Cuenta</label>
        <select class="mt-input mb-3" [(ngModel)]="form.accountId" (ngModelChange)="onAccountChange()">
          <option [ngValue]="null" disabled>Selecciona cuenta</option>
          <option *ngFor="let a of accounts()" [ngValue]="a.id">{{ a.name }}</option>
        </select>
        <ng-container *ngIf="pocketsFor(form.accountId).length > 0">
          <label class="mt-label">Pocket <span class="text-bm-muted">— de dónde sale</span></label>
          <select class="mt-input mb-3" [(ngModel)]="form.pocketId">
            <option [ngValue]="null">Sin asignar</option>
            <option *ngFor="let p of pocketsFor(form.accountId)" [ngValue]="p.id">{{ p.name }} ({{ p.balance | number: '1.2-2' }} €)</option>
          </select>
        </ng-container>
        <label class="mt-label">Fecha</label>
        <input class="mt-input mb-3" type="date" [(ngModel)]="form.date" />
        <label class="mt-label">Descripción</label>
        <input class="mt-input mb-4" type="text" [(ngModel)]="form.description" />

        <label class="mb-3 flex cursor-pointer items-center gap-2 text-sm text-bm-text">
          <input type="checkbox" class="h-4 w-4 accent-bm-cyan" [(ngModel)]="form.isRecurring" />
          🔁 Fijo (se repite cada mes)
        </label>

        <!-- Reparto y prorrateo, también al editar: cambiarlos rehace el gasto entero
             (y todas las cuotas, si está prorrateado). Se esconde cuando ya hay algún
             cobro cobrado, porque rehacerlo descuadraría la cuenta donde entró el dinero. -->
        <ng-container *ngIf="!editLocked()">
          <label class="mb-3 flex cursor-pointer items-center gap-2 text-sm text-bm-text">
            <input type="checkbox" class="h-4 w-4 accent-bm-cyan" [(ngModel)]="form.isSplit" />
            🧑‍🤝‍🧑 Compartido (lo pagáis entre varios)
          </label>
          <div *ngIf="form.isSplit" class="mb-3 rounded-lg border border-bm-border bg-bm-surface-2 p-3">
            <!-- Switch de modo: lo de siempre vs adelantarlo tú entero -->
            <div class="mb-3 flex gap-1 rounded-lg bg-bm-surface p-1">
              <button type="button" class="flex-1 rounded-md px-2 py-1.5 text-xs font-medium transition-colors"
                      [class.bg-bm-cyan]="form.splitMode === 'equal'"
                      [class.text-bm-bg]="form.splitMode === 'equal'"
                      [class.text-bm-muted]="form.splitMode !== 'equal'"
                      (click)="form.splitMode = 'equal'">
                Partes iguales
              </button>
              <button type="button" class="flex-1 rounded-md px-2 py-1.5 text-xs font-medium transition-colors"
                      [class.bg-bm-cyan]="form.splitMode === 'advance'"
                      [class.text-bm-bg]="form.splitMode === 'advance'"
                      [class.text-bm-muted]="form.splitMode !== 'advance'"
                      (click)="form.splitMode = 'advance'">
                Lo adelanto yo
              </button>
            </div>

            <ng-container *ngIf="form.splitMode === 'equal'">
              <label class="mt-label">¿Entre cuántas partes?</label>
              <input class="mt-input" type="number" min="2" max="100"
                     [ngModel]="form.splitParts" (ngModelChange)="onSplitPartsChange($event)" />

              <label class="mt-3 flex cursor-pointer items-center gap-2 text-sm text-bm-text">
                <input type="checkbox" class="h-4 w-4 accent-bm-cyan" [(ngModel)]="form.equalAdvance" />
                Lo adelanto yo y me lo devuelven
              </label>

              <p *ngIf="!form.equalAdvance" class="mt-2 text-xs text-bm-muted">
                De tu cuenta sale solo tu parte.
              </p>

              <div *ngIf="form.equalAdvance" class="mt-3">
                <label class="mt-label">¿Dónde te lo devuelven?</label>
                <select class="mt-input mb-3" [(ngModel)]="form.reimburseToAccountId">
                  <option [ngValue]="null">No me lo devuelven</option>
                  <option *ngFor="let a of accounts()" [ngValue]="a.id">{{ a.name }}</option>
                </select>

                <label class="mt-label">¿Quién te debe cada parte?</label>
                <div *ngFor="let d of form.debtorPersonIds; let i = index; trackBy: trackByIndex"
                     class="mb-2 flex items-center gap-2">
                  <span class="w-20 shrink-0 text-xs text-bm-muted">
                    {{ eachShare() | number: '1.2-2' }} €
                  </span>
                  <select class="mt-input flex-1" [ngModel]="form.debtorPersonIds[i]"
                          (ngModelChange)="form.debtorPersonIds[i] = $event">
                    <option [ngValue]="null">Sin especificar</option>
                    <option *ngFor="let p of people()" [ngValue]="p.id">{{ p.name }}</option>
                  </select>
                </div>

                <div class="mt-2 flex gap-2">
                  <input class="mt-input flex-1" type="text" [(ngModel)]="form.newPersonName"
                         placeholder="Añadir persona nueva" (keyup.enter)="addPerson()" />
                  <button type="button" class="mt-btn-ghost text-xs" (click)="addPerson()"
                          [disabled]="!form.newPersonName.trim()">
                    <i class="pi pi-plus"></i> Crear
                  </button>
                </div>
                <p class="mt-2 text-xs text-bm-muted">
                  Sale el total de la cuenta con la que pagas y cada persona queda con su
                  parte pendiente en Cobros.
                </p>
              </div>
            </ng-container>

            <ng-container *ngIf="form.splitMode === 'advance'">
              <label class="mt-label">Tu parte</label>
              <div class="mb-3 flex gap-2">
                <input class="mt-input flex-1" type="number" min="0" [(ngModel)]="form.myShareInput" placeholder="70" />
                <select class="mt-input w-20" [(ngModel)]="form.myShareUnit">
                  <option value="pct">%</option>
                  <option value="eur">€</option>
                </select>
              </div>

              <label class="mt-label">¿Dónde te devuelven el resto?</label>
              <select class="mt-input mb-3" [(ngModel)]="form.reimburseToAccountId">
                <option [ngValue]="null">No me lo devuelven</option>
                <option *ngFor="let a of accounts()" [ngValue]="a.id">{{ a.name }}</option>
              </select>

              <ng-container *ngTemplateOutlet="personPicker"></ng-container>
              <p class="mt-2 text-xs text-bm-muted">
                Sale el total de la cuenta con la que pagas y lo que no es tuyo queda pendiente de cobro.
              </p>
            </ng-container>
          </div>

          <label class="mb-3 flex cursor-pointer items-center gap-2 text-sm text-bm-text">
            <input type="checkbox" class="h-4 w-4 accent-bm-cyan" [(ngModel)]="form.isSpread" />
            📅 Prorrateado (repartido en varios meses)
          </label>
          <div *ngIf="form.isSpread" class="mb-3 rounded-lg border border-bm-border bg-bm-surface-2 p-3">
            <label class="mt-label">¿En cuántos meses?</label>
            <input class="mt-input" type="number" min="2" max="60" [(ngModel)]="form.spreadMonths" />
          </div>

          <!-- Resumen de lo que se va a guardar -->
          <div *ngIf="(form.isSplit || form.isSpread) && form.amount"
               class="mb-5 rounded-lg border border-bm-cyan/40 bg-bm-surface-2 p-3 text-sm">
            <div class="text-bm-muted">Total tecleado: <span class="font-semibold text-bm-text">{{ form.amount | number: '1.2-2' }} €</span></div>
            <div *ngIf="form.isSplit && form.splitMode === 'equal'" class="text-bm-muted">
              Tu parte ({{ form.splitParts }} partes): <span class="font-semibold text-bm-text">{{ myShare() | number: '1.2-2' }} €</span>
            </div>
            <div *ngIf="form.isSplit && form.splitMode === 'advance'" class="text-bm-muted">
              Tu parte: <span class="font-semibold text-bm-text">{{ myShare() | number: '1.2-2' }} €</span>
              <span *ngIf="sharePct() !== null"> ({{ sharePct() | number: '1.0-1' }}%)</span>
            </div>
            <div class="mt-1 font-semibold text-bm-danger">
              <ng-container *ngIf="form.isSpread">
                Te cuenta {{ perMonth() | number: '1.2-2' }} € al mes durante {{ form.spreadMonths }} meses
              </ng-container>
              <ng-container *ngIf="!form.isSpread">
                Te cuenta {{ myShare() | number: '1.2-2' }} €
              </ng-container>
            </div>
            <!-- Adelanto: el dinero que sale de más y te tienen que devolver -->
            <div *ngIf="owedToMe() > 0 && isAdvancing() && form.accountId" class="mt-2 border-t border-bm-border pt-2">
              <div class="text-bm-muted">
                Sale de <span class="font-semibold text-bm-text">{{ accountName(form.accountId!) }}</span>:
                <span class="font-semibold text-bm-danger">−{{ form.amount | number: '1.2-2' }} €</span>
              </div>
              <div *ngIf="form.reimburseToAccountId" class="text-bm-muted">
                Te deben <span class="font-semibold text-bm-warning">{{ owedToMe() | number: '1.2-2' }} €</span>
                → entrarán en <span class="font-semibold text-bm-text">{{ accountName(form.reimburseToAccountId) }}</span>
              </div>
              <div *ngIf="!form.reimburseToAccountId" class="text-xs text-bm-muted">
                Sin cuenta de destino no se guarda el cobro pendiente: elige una arriba.
              </div>
            </div>
          </div>
        </ng-container>

        <!-- Con algo ya cobrado no se puede rehacer el reparto, pero sí retocar la deuda
             que queda: lo pactado cambia (te dijeron bizum y te lo dan en efectivo). -->
        <div *ngIf="editLocked()" class="mb-3 rounded-lg border border-bm-warning/40 bg-bm-surface-2 p-3 text-xs text-bm-muted">
          🔒 Este gasto ya tiene cobros marcados como <span class="font-semibold text-bm-text">Cobrado</span>,
          así que el reparto no se puede cambiar: ese dinero ya entró en su cuenta.
          Desmárcalos en <span class="font-semibold text-bm-text">Cobros</span> si necesitas rehacerlo.
        </div>

        <div *ngIf="editLocked() && form.reimbId" class="mb-5 rounded-lg border border-bm-warning/40 bg-bm-surface-2 p-3">
          <div class="mb-3 flex items-center justify-between">
            <span class="text-sm font-semibold text-bm-text">⏳ Te deben</span>
            <button type="button" class="text-xs text-bm-danger hover:underline" (click)="removeReimbursement()">
              Quitar
            </button>
          </div>

          <label class="mt-label">¿Cuánto?</label>
          <input class="mt-input mb-3" type="number" min="0" step="0.01" [(ngModel)]="form.reimbAmount" />

          <label class="mt-label">¿Dónde te lo devuelven?</label>
          <select class="mt-input mb-3" [(ngModel)]="form.reimburseToAccountId">
            <option *ngFor="let a of accounts()" [ngValue]="a.id">{{ a.name }}</option>
          </select>

          <ng-container *ngTemplateOutlet="personPicker"></ng-container>
        </div>

        <button class="mt-btn-cyan w-full" (click)="save()"><i class="pi pi-save"></i> {{ editingId() ? 'Guardar cambios' : 'Guardar gasto' }}</button>
      </div>
    </div>

    <!-- Modal de alcance al borrar una cuota de un prorrateo -->
    <div *ngIf="spreadAsk() as ask" class="fixed inset-0 z-[60] flex items-center justify-center bg-black/60 p-4"
         (click)="spreadAsk.set(null)">
      <div class="mt-card w-full max-w-sm p-6" (click)="$event.stopPropagation()">
        <h2 class="mb-1 text-lg font-semibold text-bm-text">Borrar gasto prorrateado</h2>
        <p class="mb-5 text-sm text-bm-muted">
          Este gasto es la cuota {{ ask.spreadIndex }} de {{ ask.spreadCount }}. ¿Qué quieres borrar?
        </p>
        <button class="mt-btn-ghost mb-2 w-full justify-center" (click)="applySpreadScope('one')">
          Solo esta cuota
        </button>
        <button class="mt-btn-danger w-full justify-center" (click)="applySpreadScope('all')">
          Las {{ ask.spreadCount }} cuotas
        </button>
      </div>
    </div>

    <!-- Selector de persona, compartido por el alta y la edición del reembolso -->
    <ng-template #personPicker>
      <label class="mt-label">¿Quién te lo debe?</label>
      <select class="mt-input" [(ngModel)]="form.personId">
        <option [ngValue]="null">Sin especificar</option>
        <option *ngFor="let p of people()" [ngValue]="p.id">{{ p.name }}</option>
        <option [ngValue]="NEW_PERSON">➕ Nueva persona…</option>
      </select>
      <div *ngIf="form.personId === NEW_PERSON" class="mt-2 flex gap-2">
        <input class="mt-input flex-1" type="text" [(ngModel)]="form.newPersonName"
               placeholder="Nombre" (keyup.enter)="addPerson()" />
        <button type="button" class="mt-btn-ghost text-xs" (click)="addPerson()"
                [disabled]="!form.newPersonName.trim()">
          <i class="pi pi-check"></i> Añadir
        </button>
      </div>
    </ng-template>
  `,
})
export class ExpensesComponent implements OnInit {
  private finance = inject(FinanceService);
  private msg = inject(MessageService);

  items = signal<Expense[]>([]);
  accounts = signal<Account[]>([]);
  categories = signal<Category[]>([]);
  pockets = signal<Pocket[]>([]);
  reimbursements = signal<Reimbursement[]>([]);
  people = signal<Person[]>([]);
  pets = signal<Pet[]>([]);
  showAdd = signal(false);

  /** Valor centinela del desplegable de personas para "crear una nueva". */
  readonly NEW_PERSON = -1;

  /** Categoria de Mascotas: sus gastos se mandan a PetRecording. 0 = desactivado. */
  private readonly PET_CATEGORY_ID = environment.petCategoryId;
  editingId = signal<number | null>(null);
  spreadAsk = signal<Expense | null>(null);
  form: Form = this.empty();

  /** El gasto que se edita tiene algún cobro ya cobrado: el reparto no se toca. */
  editLocked = signal(false);

  /** El gasto que se edita ya era compartido o prorrateado. */
  private editWasStructural = false;

  /** Huella del reparto al abrir el modal, para saber si de verdad ha cambiado. */
  private editSignature = '';

  /**
   * Lo que pagas tú del total tecleado. A partes iguales es total/N; con reparto
   * libre es lo que hayas dicho, en % o en €.
   */
  myShare(): number {
    const f = this.form;
    const total = Number(f.amount) || 0;
    if (!f.isSplit) return total;

    if (f.splitMode === 'equal') {
      if (f.splitParts < 2) return total;
      return Math.round((total / f.splitParts) * 100) / 100;
    }

    const typed = Number(f.myShareInput);
    if (!typed || typed <= 0) return total;
    const share = f.myShareUnit === 'pct' ? (total * typed) / 100 : typed;
    // Tu parte nunca puede pasar del total.
    return Math.min(Math.round(share * 100) / 100, total);
  }

  /** Tu parte en porcentaje, para enseñarlo en el resumen. null si no aplica. */
  sharePct(): number | null {
    const total = Number(this.form.amount) || 0;
    if (total <= 0) return null;
    return (this.myShare() / total) * 100;
  }

  /** Lo que adelantas de los demás y te tienen que devolver. */
  owedToMe(): number {
    const total = Number(this.form.amount) || 0;
    return Math.round((total - this.myShare()) * 100) / 100;
  }

  /** Lo que te debe cada persona en el reparto a partes iguales. */
  eachShare(): number {
    const n = this.form.debtorPersonIds.length;
    if (n < 1) return 0;
    return Math.floor((this.owedToMe() / n) * 100) / 100;
  }

  /** Al cambiar el nº de partes, ajustamos la lista de deudores a partes − 1. */
  onSplitPartsChange(parts: number): void {
    this.form.splitParts = parts;
    const needed = Math.max(0, (Number(parts) || 2) - 1);
    const current = this.form.debtorPersonIds;

    if (needed > current.length) {
      this.form.debtorPersonIds = [...current, ...Array(needed - current.length).fill(null)];
    } else if (needed < current.length) {
      this.form.debtorPersonIds = current.slice(0, needed);
    }
  }

  trackByIndex(index: number): number {
    return index;
  }

  /** ¿Estás adelantando el dinero de los demás? Vale para los dos modos de reparto. */
  isAdvancing(): boolean {
    const f = this.form;
    if (!f.isSplit) return false;
    return f.splitMode === 'advance' || f.equalAdvance;
  }

  /** Lo que te cuenta cada mes una vez prorrateada tu parte. */
  perMonth(): number {
    const f = this.form;
    if (!f.isSpread || f.spreadMonths < 2) return this.myShare();
    return Math.floor((this.myShare() / f.spreadMonths) * 100) / 100;
  }

  readonly modes: { key: PeriodMode; label: string }[] = [
    { key: 'month', label: 'Mes' },
    { key: 'year', label: 'Año' },
    { key: 'all', label: 'Todo' },
  ];
  periodMode = signal<PeriodMode>('month');
  month = signal<string>(new Date().toISOString().substring(0, 7));
  year = signal<number>(new Date().getFullYear());
  catFilter = signal<number | null>(null);
  accFilter = signal<number | null>(null);
  /** null = todos los sobres; 'none' = los gastos que no salen de ningún sobre. */
  pocketFilter = signal<number | 'none' | null>(null);

  /** Sobres que se pueden filtrar: los de la cuenta elegida, o todos si no hay cuenta. */
  filterPockets = computed(() => {
    const accountId = this.accFilter();
    return accountId == null
      ? this.pockets()
      : this.pockets().filter((p) => p.accountId === accountId);
  });

  private catIndex = computed(() => new Map(this.categories().map((c) => [c.id, c])));

  /** Reembolsos de cada gasto (puede haber uno por persona), para pintar su fila. */
  reimbByExpense = computed(() => {
    const byExpense = new Map<number, Reimbursement[]>();
    for (const r of this.reimbursements()) {
      const arr = byExpense.get(r.expenseId) ?? [];
      arr.push(r);
      byExpense.set(r.expenseId, arr);
    }
    return byExpense;
  });

  /** Lo que te deben todavía, para la tarjeta de arriba. */
  pendingReimbursements = computed(() => this.reimbursements().filter((r) => !r.settledAt));
  pendingTotal = computed(() => this.pendingReimbursements().reduce((s, r) => s + Number(r.amount), 0));

  private filtered = computed(() =>
    this.items().filter(
      (e) =>
        this.inPeriod(e.expenseDate) &&
        this.matchesCat(e.categoryId) &&
        (this.accFilter() == null || e.accountId === this.accFilter()) &&
        this.matchesPocket(e.pocketId),
    ),
  );
  total = computed(() => this.filtered().reduce((s, e) => s + Number(e.amount), 0));

  groups = computed<AccountGroup[]>(() => {
    const byAcc = new Map<number, Expense[]>();
    for (const e of this.filtered()) {
      const arr = byAcc.get(e.accountId) ?? [];
      arr.push(e);
      byAcc.set(e.accountId, arr);
    }
    return [...byAcc.entries()]
      .map(([accountId, list]) => ({
        accountId,
        name: this.accountName(accountId),
        subtotal: list.reduce((s, e) => s + Number(e.amount), 0),
        items: list.sort((a, b) => (b.expenseDate || '').localeCompare(a.expenseDate || '')),
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
    this.finance.getExpenseCategories().subscribe({ next: (r) => this.categories.set(r) });
    this.finance.getPockets().subscribe({ next: (r) => this.pockets.set(r) });
    if (this.PET_CATEGORY_ID > 0)
      this.finance.getPets().subscribe({ next: (r) => this.pets.set(r) });
  }

  /** Pockets de la cuenta seleccionada en el formulario. */
  pocketsFor(accountId: number | null): Pocket[] {
    if (accountId == null) return [];
    return this.pockets().filter((p) => p.accountId === accountId);
  }

  pocketName(id: number | null | undefined): string {
    return this.pockets().find((p) => p.id === id)?.name ?? '';
  }

  /** Sin cuenta elegida puede haber sobres con el mismo nombre en cuentas distintas. */
  pocketFilterLabel(pocket: Pocket): string {
    return this.accFilter() == null
      ? `${this.accountName(pocket.accountId)} · ${pocket.name}`
      : pocket.name;
  }

  /** Al cambiar de cuenta, el sobre que estaba filtrado puede no ser suyo. */
  onAccFilterChange(accountId: number | null): void {
    this.accFilter.set(accountId);
    const pocketId = this.pocketFilter();
    if (typeof pocketId === 'number' && !this.filterPockets().some((p) => p.id === pocketId))
      this.pocketFilter.set(null);
  }

  /**
   * ¿La categoría elegida es la de Mascotas, o cuelga de ella? Las subcategorías
   * (Veterinario, Comida…) también son gastos de mascota.
   */
  isPetCategory(categoryId: number | null): boolean {
    if (this.PET_CATEGORY_ID <= 0 || categoryId == null) return false;

    const index = this.catIndex();
    let cur: number | null | undefined = categoryId;
    const seen = new Set<number>();
    while (cur != null && !seen.has(cur)) {
      if (cur === this.PET_CATEGORY_ID) return true;
      seen.add(cur);
      cur = index.get(cur)?.parentId ?? null;
    }
    return false;
  }

  petName(id: number | null | undefined): string {
    return this.pets().find((p) => p.id === id)?.name ?? '';
  }

  /** Da de alta una mascota sin salir del modal y la deja seleccionada. */
  addPet(): void {
    const name = this.form.newPetName.trim();
    const id = Number(this.form.newPetId);
    if (!name || !id) return;

    this.finance.createPet(id, name).subscribe({
      next: (pet) => {
        // El backend devuelve la que hay si ya estaba, así que evitamos duplicarla aquí.
        if (!this.pets().some((p) => p.id === pet.id)) {
          this.pets.set([...this.pets(), pet].sort((a, b) => a.name.localeCompare(b.name)));
        }
        this.form.petId = pet.id;
        this.form.newPetId = null;
        this.form.newPetName = '';
      },
      // El backend avisa si ese id ya es de otra mascota: ese mensaje vale más que uno genérico.
      error: (e) =>
        this.msg.add({
          severity: 'error',
          summary: 'Error',
          detail: e?.error?.ErrorMessage ?? 'No se pudo crear la mascota',
        }),
    });
  }

  onAccountChange(): void {
    this.form.pocketId = null; // los pockets dependen de la cuenta
  }

  load(): void {
    this.finance.getExpenses().subscribe({ next: (r) => this.items.set(r) });
    // Los saldos de los pockets cambian al crear/borrar gastos que salen de un pocket.
    this.finance.getPockets().subscribe({ next: (r) => this.pockets.set(r) });
    this.finance.getReimbursements().subscribe({ next: (r) => this.reimbursements.set(r) });
    this.finance.getPeople().subscribe({ next: (r) => this.people.set(r) });
  }

  /** Ya te han hecho el bizum: el dinero entra en la cuenta de destino. */
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

  /** Deshacer un cobro marcado por error. */
  unsettle(r: Reimbursement): void {
    this.finance.unsettleReimbursement(r.id).subscribe({
      next: () => this.load(),
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo deshacer' }),
    });
  }

  accountName(id: number): string {
    return this.accounts().find((a) => a.id === id)?.name ?? `cuenta ${id}`;
  }

  categoryName(id: number): string {
    return this.categories().find((c) => c.id === id)?.name ?? `cat ${id}`;
  }

  /** Descripción del gasto que originó un reembolso. */
  expenseDescription(expenseId: number): string {
    return this.items().find((e) => e.id === expenseId)?.description || '(sin descripción)';
  }

  /** Nombre de la persona seleccionada en el formulario, para guardarlo con el reembolso. */
  private selectedPersonName(): string | null {
    const id = this.form.personId;
    if (id == null || id === this.NEW_PERSON) return null;
    return this.people().find((p) => p.id === id)?.name ?? null;
  }

  /** Crea una persona sin salir del modal y la deja seleccionada. */
  addPerson(): void {
    const name = this.form.newPersonName.trim();
    if (!name) return;

    this.finance.createPerson(name).subscribe({
      next: (person) => {
        // El backend devuelve la existente si ya la tenías, así que evitamos duplicarla aquí.
        if (!this.people().some((p) => p.id === person.id)) {
          this.people.set([...this.people(), person].sort((a, b) => a.name.localeCompare(b.name)));
        }
        this.form.personId = person.id;
        this.form.newPersonName = '';
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo crear la persona' }),
    });
  }

  openAdd(): void {
    this.editingId.set(null);
    this.editLocked.set(false);
    this.editWasStructural = false;
    this.form = this.empty();
    this.editSignature = this.splitSignature();
    this.showAdd.set(true);
  }

  openEdit(expense: Expense): void {
    this.editingId.set(expense.id);

    // El reparto se edita entero, no solo la cuota que has pinchado: si el gasto está
    // prorrateado, las hermanas van en el mismo lote.
    const siblings = expense.spreadGroup
      ? this.items().filter((x) => x.spreadGroup === expense.spreadGroup)
      : [expense];
    const reimbs = siblings.flatMap((x) => this.reimbByExpense().get(x.id) ?? []);

    // Con algo ya cobrado el reparto queda bloqueado: rehacerlo es borrar y volver a
    // crear, y ese dinero ya entró en la cuenta de destino. Solo se deja retocar la deuda.
    this.editLocked.set(reimbs.some((r) => !!r.settledAt));

    // El formulario trabaja siempre con el TOTAL, igual que al crear: tu parte más lo
    // que adelantaste. En un prorrateo tu parte es la suma de todas las cuotas.
    const myShareTotal = siblings.reduce((sum, x) => sum + Number(x.amount), 0);
    const owed = reimbs.reduce((sum, r) => sum + Number(r.amount), 0);
    const isSplit = expense.totalAmount != null;
    const total = isSplit ? Number(expense.totalAmount) : myShareTotal + owed;

    // A partes iguales hay una deuda por cada parte que no es tuya, en orden.
    const isEqual = expense.splitParts != null;
    const parts = expense.splitParts ?? 2;
    const debtorPersonIds: (number | null)[] = isEqual
      ? Array.from({ length: Math.max(1, parts - 1) }, (_, i) => reimbs[i]?.personId ?? null)
      : [null];

    this.editWasStructural = isSplit || !!expense.spreadGroup;

    // El bloque de retoque de la deuda es para una sola: con varias se gestionan en Cobros.
    const reimb = reimbs.length === 1 ? reimbs[0] : null;
    this.form = {
      accountId: expense.accountId,
      categoryId: expense.categoryId,
      amount: total,
      description: expense.description ?? '',
      date: (expense.expenseDate || '').substring(0, 10),
      isRecurring: expense.isRecurring ?? false,
      pocketId: expense.pocketId ?? null,
      petId: expense.petId ?? null,
      newPetId: null,
      newPetName: '',
      isSplit,
      splitMode: isEqual ? 'equal' : 'advance',
      splitParts: parts,
      equalAdvance: isEqual && reimbs.length > 0,
      debtorPersonIds,
      // En reparto libre tu parte se reinyecta en euros: el % se recalcularía mal si el
      // total tecleado cambia, y en euros es exactamente lo que se guardó.
      myShareInput: isSplit && !isEqual ? myShareTotal : null,
      myShareUnit: 'eur',
      reimburseToAccountId: reimbs[0]?.toAccountId ?? null,
      personId: reimbs[0]?.personId ?? null,
      newPersonName: '',
      isSpread: !!expense.spreadGroup,
      spreadMonths: expense.spreadCount ?? 3,
      reimbId: reimb?.id ?? null,
      reimbAmount: reimb ? Number(reimb.amount) : null,
    };
    this.editSignature = this.splitSignature();
    this.showAdd.set(true);
  }

  /**
   * El mensaje que manda el backend, venga como `{ error }` (los 400 del endpoint) o
   * dentro del `HttpResponse.Fail` del middleware global. null si no hay ninguno útil.
   */
  private apiError(err: unknown): string | null {
    const body = (err as { error?: unknown } | null)?.error;
    if (!body || typeof body !== 'object') return null;

    const fields = body as Record<string, unknown>;
    const msg = fields['error'] ?? fields['ErrorMessage'] ?? fields['errorMessage'];
    return typeof msg === 'string' && msg.trim() ? msg : null;
  }

  /**
   * Huella de todo lo que define el reparto. Si al guardar no ha cambiado, el gasto se
   * actualiza con el PUT de siempre; si ha cambiado, hay que rehacerlo. Así editar la
   * descripción de una cuota no rehace las seis de un prorrateo.
   */
  private splitSignature(): string {
    const f = this.form;
    return JSON.stringify([
      f.isSplit,
      f.splitMode,
      f.splitParts,
      f.equalAdvance,
      f.reimburseToAccountId,
      f.debtorPersonIds,
      f.personId,
      Number(f.myShareInput) || 0,
      f.myShareUnit,
      f.isSpread,
      f.spreadMonths,
      Number(f.amount) || 0,
    ]);
  }

  /** Quita el cobro pendiente sin borrar el gasto: al final lo pagaste tú entero. */
  removeReimbursement(): void {
    const id = this.form.reimbId;
    if (id == null) return;

    this.finance.deleteReimbursement(id).subscribe({
      next: () => {
        this.form.reimbId = null;
        this.form.reimbAmount = null;
        this.msg.add({ severity: 'success', summary: 'Quitado', detail: 'Ya no te deben nada de este gasto' });
        this.load();
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo quitar' }),
    });
  }

  save(): void {
    const f = this.form;
    if (!f.accountId || !f.categoryId || !f.amount) {
      this.msg.add({ severity: 'warn', summary: 'Faltan datos', detail: 'Rellena cuenta, categoría e importe' });
      return;
    }
    const id = this.editingId();
    // No se comprueba el saldo del pocket: si no llega, se queda en negativo. El gasto
    // ya lo has pagado, y apuntarlo en otro sitio "porque no había fondos" descuadra más.
    const body: Partial<Expense> = {
      accountId: f.accountId,
      categoryId: f.categoryId,
      amount: f.amount,
      description: f.description,
      expenseDate: f.date,
      isRecurring: f.isRecurring,
      pocketId: f.pocketId,
      // Fuera de la categoría de Mascotas no viaja mascota: así, si sacas un gasto
      // de ahí al editarlo, el backend lo borra también en PetRecording.
      petId: this.isPetCategory(f.categoryId) ? f.petId : null,
    };
    // Reparto y/o prorrateo: el backend divide y reparte (amount = total tecleado).
    // Al editar solo se rehace si el reparto ha cambiado de verdad; si no, basta el PUT
    // de siempre, y así retocar la descripción de una cuota no rehace todo el prorrateo.
    const structural = f.isSplit || f.isSpread || this.editWasStructural;
    if (structural && (!id || this.splitSignature() !== this.editSignature)) {
      if (id && this.editLocked()) {
        this.msg.add({
          severity: 'warn',
          summary: 'No se puede cambiar el reparto',
          detail: 'Este gasto ya tiene cobros cobrados. Desmárcalos en Cobros y vuelve a intentarlo.',
        });
        return;
      }

      const isEqual = f.isSplit && f.splitMode === 'equal';
      const isAdvance = f.isSplit && f.splitMode === 'advance';
      // A partes iguales también puedes adelantarlo tú, diciendo quién te debe cada parte.
      const equalAdvance = isEqual && f.equalAdvance;
      const parts = isEqual ? f.splitParts : null;
      const months = f.isSpread ? f.spreadMonths : 1;
      const owed = this.owedToMe();
      const wantsReimbursement = (isAdvance || equalAdvance) && owed > 0;

      const opts = {
        myShareAmount: isAdvance ? this.myShare() : null,
        reimburseToAccountId: wantsReimbursement ? f.reimburseToAccountId : null,
        debtor: isAdvance ? this.selectedPersonName() : null,
        personId: isAdvance && f.personId !== this.NEW_PERSON ? f.personId : null,
        debtorPersonIds: equalAdvance ? f.debtorPersonIds : null,
      };

      const split$ = id
        ? this.finance.updateSplitSpreadExpense(id, body, parts, months, opts)
        : this.finance.createSplitSpreadExpense(body, parts, months, opts);

      split$.subscribe({
        next: (cuotas) => {
          this.showAdd.set(false);
          const detail = cuotas.length > 1
            ? `${cuotas.length} cuotas de ${cuotas[0].amount} €`
            : `${cuotas[0].amount} € (tu parte)`;
          const extra = wantsReimbursement && owed > 0 ? ` · te deben ${owed.toFixed(2)} €` : '';
          this.msg.add({
            severity: 'success',
            summary: id ? 'Gasto actualizado' : 'Gasto añadido',
            detail: detail + extra,
          });
          this.load();
        },
        error: (err) =>
          this.msg.add({
            severity: 'error',
            summary: 'Error',
            // El backend dice por qué (partes y personas que no cuadran, cobros ya
            // cobrados...), y ese mensaje ayuda mucho más que uno genérico.
            detail: this.apiError(err) ?? (id ? 'No se pudo cambiar el reparto' : 'No se pudo crear el gasto'),
          }),
      });
      return;
    }

    const req$ = id ? this.finance.updateExpense(id, body) : this.finance.createExpense(body);
    req$.subscribe({
      next: () => {
        // Solo con el reparto bloqueado: ahí el bloque de retoque de la deuda es lo
        // único que se ve, y lo pactado puede haber cambiado (efectivo en vez de bizum,
        // otro importe, otra persona). Si no está bloqueado, eso lo lleva el reparto.
        if (id && f.reimbId && this.editLocked()) {
          this.saveReimbursementChanges(f.reimbId);
          return;
        }
        this.showAdd.set(false);
        this.msg.add({ severity: 'success', summary: id ? 'Gasto actualizado' : 'Gasto añadido', detail: '' });
        this.load();
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: '¿Ids de cuenta/categoría válidos?' }),
    });
  }

  /** Guarda los cambios del reembolso al editar el gasto. */
  private saveReimbursementChanges(reimbId: number): void {
    const f = this.form;
    this.finance
      .updateReimbursement(reimbId, {
        amount: f.reimbAmount != null ? Number(f.reimbAmount) : undefined,
        toAccountId: f.reimburseToAccountId ?? undefined,
        // 0 = quitar la persona; NEW_PERSON es un estado intermedio, no se manda.
        personId: f.personId === this.NEW_PERSON ? undefined : (f.personId ?? 0),
        // Cadena vacía = borrar el nombre guardado, para que no quede uno viejo colgando.
        debtor: f.personId === this.NEW_PERSON ? undefined : (this.selectedPersonName() ?? ''),
      })
      .subscribe({
        next: () => {
          this.showAdd.set(false);
          this.msg.add({ severity: 'success', summary: 'Gasto actualizado', detail: '' });
          this.load();
        },
        error: () =>
          this.msg.add({ severity: 'error', summary: 'Error', detail: 'El gasto se guardó, pero el reembolso no' }),
      });
  }

  remove(expense: Expense): void {
    // Si es una cuota de un prorrateo, preguntamos si borrar solo esa o todas.
    if (expense.spreadGroup) {
      this.spreadAsk.set(expense);
      return;
    }
    this.deleteOne(expense.id);
  }

  /** Resuelve el modal de borrado de prorrateo. */
  applySpreadScope(scope: 'one' | 'all'): void {
    const expense = this.spreadAsk();
    if (!expense) return;
    this.spreadAsk.set(null);

    if (scope === 'one') {
      this.deleteOne(expense.id);
      return;
    }
    this.finance.deleteExpenseSpread(expense.id).subscribe({
      next: (r) => {
        this.msg.add({ severity: 'info', summary: 'Prorrateo eliminado', detail: `${r.deleted} cuotas borradas` });
        this.load();
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo eliminar el prorrateo' }),
    });
  }

  private deleteOne(id: number): void {
    this.finance.deleteExpense(id).subscribe({
      next: () => {
        this.msg.add({ severity: 'info', summary: 'Gasto eliminado', detail: '' });
        this.load();
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo eliminar' }),
    });
  }

  /** ¿El gasto sale del sobre filtrado? 'none' = los que no salen de ninguno. */
  private matchesPocket(pocketId: number | null | undefined): boolean {
    const target = this.pocketFilter();
    if (target == null) return true;
    if (target === 'none') return pocketId == null;
    return pocketId === target;
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
      pocketId: null,
      petId: null,
      newPetId: null,
      newPetName: '',
      isSplit: false,
      splitMode: 'equal',
      splitParts: 2,
      equalAdvance: false,
      debtorPersonIds: [null],
      myShareInput: null,
      myShareUnit: 'pct',
      reimburseToAccountId: null,
      personId: null,
      newPersonName: '',
      isSpread: false,
      spreadMonths: 3,
      reimbId: null,
      reimbAmount: null,
    };
  }
}
