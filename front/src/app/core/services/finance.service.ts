import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Expense, Income, Reimbursement } from '../models/movement.model';
import { Account } from '../models/account.model';
import { AccountType } from '../models/account-type.model';
import { Category } from '../models/category.model';
import { Transfer } from '../models/transfer.model';
import { Pocket } from '../models/pocket.model';
import { Pet } from '../models/pet.model';
import { Budget } from '../models/budget.model';
import { Person } from '../models/person.model';

/** Lo que acompaña al reparto de un gasto, tanto al crearlo como al rehacerlo. */
export interface SplitOptions {
  /** Reparto libre: tu parte en euros (un 70/30). Excluyente con `splitParts`. */
  myShareAmount?: number | null;
  /** Cuenta donde te devuelven lo que adelantaste. Sin ella no se guarda el cobro. */
  reimburseToAccountId?: number | null;
  debtor?: string | null;
  personId?: number | null;
  /** A partes iguales: quién te debe cada parte. null = sin especificar. */
  debtorPersonIds?: (number | null)[] | null;
}

@Injectable({ providedIn: 'root' })
export class FinanceService {
  private http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  getIncomes(): Observable<Income[]> {
    return this.http.get<Income[]>(`${this.base}/incomes`);
  }
  createIncome(body: Partial<Income>): Observable<Income> {
    return this.http.post<Income>(`${this.base}/incomes`, body);
  }
  updateIncome(id: number, body: Partial<Income>): Observable<Income> {
    return this.http.put<Income>(`${this.base}/incomes/${id}`, body);
  }
  deleteIncome(id: number): Observable<unknown> {
    return this.http.delete(`${this.base}/incomes/${id}`);
  }
  /** Crea un ingreso fijo materializando `monthsAhead` ocurrencias mensuales. */
  createRecurringIncome(body: Partial<Income>, monthsAhead = 12): Observable<{ created: number }> {
    return this.http.post<{ created: number }>(
      `${this.base}/incomes/recurring?monthsAhead=${monthsAhead}`,
      body,
    );
  }
  /** "Editar este y siguientes": cambia el importe de la serie desde este ingreso. */
  updateIncomeSeries(id: number, amount: number): Observable<{ updated: number }> {
    return this.http.put<{ updated: number }>(`${this.base}/incomes/${id}/series`, { amount });
  }
  /** "Borrar este y siguientes": elimina la serie desde este ingreso. */
  deleteIncomeSeries(id: number): Observable<{ deleted: number }> {
    return this.http.delete<{ deleted: number }>(`${this.base}/incomes/${id}/series`);
  }

  getExpenses(): Observable<Expense[]> {
    return this.http.get<Expense[]>(`${this.base}/expenses`);
  }
  createExpense(body: Partial<Expense>): Observable<Expense> {
    return this.http.post<Expense>(`${this.base}/expenses`, body);
  }
  updateExpense(id: number, body: Partial<Expense>): Observable<Expense> {
    return this.http.put<Expense>(`${this.base}/expenses/${id}`, body);
  }
  deleteExpense(id: number): Observable<unknown> {
    return this.http.delete(`${this.base}/expenses/${id}`);
  }
  /**
   * Crea un gasto compartido y/o prorrateado en `spreadMonths` cuotas. El `amount`
   * del body es siempre el importe TOTAL.
   *
   * Tu parte se indica de una de estas dos formas, nunca las dos a la vez:
   * `splitParts` (a partes iguales, pagas 1 de N) o `myShareAmount` (reparto libre).
   * Si además adelantaste el dinero de los demás, `reimburseToAccountId` dice en qué
   * cuenta te lo devolverán y queda pendiente de cobro.
   */
  createSplitSpreadExpense(
    body: Partial<Expense>,
    splitParts: number | null,
    spreadMonths = 1,
    opts: SplitOptions = {},
  ): Observable<Expense[]> {
    return this.http.post<Expense[]>(
      `${this.base}/expenses/split?${this.splitParams(splitParts, spreadMonths, opts)}`,
      body,
    );
  }

  /**
   * Rehace el reparto de un gasto que ya existe: otro número de partes, otras personas,
   * adelantarlo o dejar de adelantarlo, otro número de meses. Mismos parámetros que el
   * alta, y el `amount` del body vuelve a ser el TOTAL.
   *
   * Si el gasto está prorrateado se rehace el grupo entero. Por dentro es borrar y
   * volver a crear, así que las cuotas que devuelve tienen **ids nuevos**: hay que
   * recargar la lista. Responde 403 si alguna deuda ya está marcada como cobrada.
   */
  updateSplitSpreadExpense(
    id: number,
    body: Partial<Expense>,
    splitParts: number | null,
    spreadMonths = 1,
    opts: SplitOptions = {},
  ): Observable<Expense[]> {
    return this.http.put<Expense[]>(
      `${this.base}/expenses/${id}/split?${this.splitParams(splitParts, spreadMonths, opts)}`,
      body,
    );
  }

  /** Los parámetros de query que comparten el alta y la edición del reparto. */
  private splitParams(splitParts: number | null, spreadMonths: number, opts: SplitOptions): URLSearchParams {
    const params = new URLSearchParams({ spreadMonths: String(spreadMonths) });
    if (splitParts) params.set('splitParts', String(splitParts));
    if (opts.myShareAmount != null) params.set('myShareAmount', String(opts.myShareAmount));
    if (opts.reimburseToAccountId != null) params.set('reimburseToAccountId', String(opts.reimburseToAccountId));
    if (opts.debtor) params.set('debtor', opts.debtor);
    if (opts.personId != null) params.set('personId', String(opts.personId));
    // El backend espera "3,0,7", donde 0 es "sin especificar".
    if (opts.debtorPersonIds?.length) {
      params.set('debtorPersonIds', opts.debtorPersonIds.map((id) => id ?? 0).join(','));
    }
    return params;
  }

  // --- Personas con las que compartes gastos (agenda) ---

  getPeople(): Observable<Person[]> {
    return this.http.get<Person[]>(`${this.base}/people`);
  }
  /** Si ya existe alguien con ese nombre, el backend devuelve el que hay en vez de duplicar. */
  createPerson(name: string): Observable<Person> {
    return this.http.post<Person>(`${this.base}/people`, { name });
  }
  deletePerson(id: number): Observable<unknown> {
    return this.http.delete(`${this.base}/people/${id}`);
  }

  // --- Reembolsos: lo que adelantaste de otra gente y te tienen que devolver ---

  getReimbursements(): Observable<Reimbursement[]> {
    return this.http.get<Reimbursement[]>(`${this.base}/reimbursements`);
  }
  /**
   * Cambia lo pactado: te dijeron bizum y al final es efectivo, te devuelven otra
   * cantidad, o te lo debe otra persona. Lo que no mandes se queda como está.
   * `personId: 0` quita la persona.
   */
  updateReimbursement(
    id: number,
    body: { amount?: number; toAccountId?: number; personId?: number; debtor?: string | null },
  ): Observable<unknown> {
    return this.http.patch(`${this.base}/reimbursements/${id}`, body);
  }
  /** Ya te han hecho el bizum: el dinero entra en la cuenta de destino. */
  settleReimbursement(id: number): Observable<unknown> {
    return this.http.post(`${this.base}/reimbursements/${id}/settle`, {});
  }
  /** Deshacer: vuelve a quedar pendiente de cobro. */
  unsettleReimbursement(id: number): Observable<unknown> {
    return this.http.post(`${this.base}/reimbursements/${id}/unsettle`, {});
  }
  deleteReimbursement(id: number): Observable<unknown> {
    return this.http.delete(`${this.base}/reimbursements/${id}`);
  }
  /** Borra de golpe todas las cuotas del prorrateo al que pertenece este gasto. */
  deleteExpenseSpread(id: number): Observable<{ deleted: number }> {
    return this.http.delete<{ deleted: number }>(`${this.base}/expenses/${id}/spread`);
  }

  // --- Transferencias entre cuentas ---

  getTransfers(): Observable<Transfer[]> {
    return this.http.get<Transfer[]>(`${this.base}/transfers`);
  }
  createTransfer(body: { fromAccountId: number; toAccountId: number; amount: number; transferDate?: string; description?: string }): Observable<Transfer> {
    return this.http.post<Transfer>(`${this.base}/transfers`, body);
  }
  deleteTransfer(id: number): Observable<unknown> {
    return this.http.delete(`${this.base}/transfers/${id}`);
  }

  // --- Mascotas (tabla auxiliar; los ids son los de PetRecording) ---

  getPets(): Observable<Pet[]> {
    return this.http.get<Pet[]>(`${this.base}/pets`);
  }

  /** El id NO es opcional ni lo genera la BD: es el que tiene en PetRecording. */
  createPet(id: number, name: string): Observable<Pet> {
    return this.http.post<Pet>(`${this.base}/pets`, { id, name });
  }

  // --- Pockets (sub-saldos dentro de una cuenta) ---

  getPockets(): Observable<Pocket[]> {
    return this.http.get<Pocket[]>(`${this.base}/pockets`);
  }
  createPocket(body: { accountId: number; name: string; balance: number }): Observable<Pocket> {
    return this.http.post<Pocket>(`${this.base}/pockets`, body);
  }
  updatePocket(id: number, name: string): Observable<unknown> {
    return this.http.patch(`${this.base}/pockets/${id}`, { name });
  }
  deletePocket(id: number): Observable<unknown> {
    return this.http.delete(`${this.base}/pockets/${id}`);
  }
  /** Mueve dinero entre pockets (o entre "sin asignar" y un pocket). null = sin asignar. */
  movePocket(body: { fromPocketId: number | null; toPocketId: number | null; amount: number }): Observable<unknown> {
    return this.http.post(`${this.base}/pockets/move`, body);
  }

  // --- Presupuestos por categoría (importe mensual) ---

  getBudgets(): Observable<Budget[]> {
    return this.http.get<Budget[]>(`${this.base}/budgets`);
  }
  /** Crea o actualiza el presupuesto mensual de una categoría (upsert por categoría). */
  upsertBudget(body: { categoryId: number; monthlyLimit: number }): Observable<Budget> {
    return this.http.post<Budget>(`${this.base}/budgets`, body);
  }
  deleteBudget(id: number): Observable<unknown> {
    return this.http.delete(`${this.base}/budgets/${id}`);
  }

  /** Genera los movimientos fijos que falten en el mes actual. */
  generateRecurring(): Observable<{ created: number }> {
    return this.http.post<{ created: number }>(`${this.base}/recurring/generate`, {});
  }

  // --- Listados para los desplegables ---

  getAccounts(): Observable<Account[]> {
    return this.http.get<Account[]>(`${this.base}/accounts`);
  }

  getAccountTypes(): Observable<AccountType[]> {
    return this.http.get<AccountType[]>(`${this.base}/account-types`);
  }

  createAccount(body: { accountTypeId: number; name: string; balance: number }): Observable<Account> {
    return this.http.post<Account>(`${this.base}/accounts`, body);
  }

  updateAccount(id: number, body: { accountTypeId: number; name: string; balance: number }): Observable<unknown> {
    return this.http.patch(`${this.base}/accounts/${id}`, body);
  }

  deleteAccount(id: number): Observable<unknown> {
    return this.http.delete(`${this.base}/accounts/${id}`);
  }

  /** `isCommon` (categoría común para todos) solo lo respeta el backend si eres admin. */
  createIncomeCategory(name: string, parentId: number | null, isCommon = false): Observable<Category> {
    return this.http.post<Category>(`${this.base}/income-categories`, { name, parentId, isCommon });
  }

  updateIncomeCategory(id: number, name: string, isCommon?: boolean): Observable<unknown> {
    return this.http.patch(`${this.base}/income-categories/${id}`, { name, isCommon });
  }

  deleteIncomeCategory(id: number): Observable<unknown> {
    return this.http.delete(`${this.base}/income-categories/${id}`);
  }

  /** `isCommon` (categoría común para todos) solo lo respeta el backend si eres admin. */
  createExpenseCategory(name: string, parentId: number | null, isCommon = false): Observable<Category> {
    return this.http.post<Category>(`${this.base}/expense-categories`, { name, parentId, isCommon });
  }

  updateExpenseCategory(id: number, name: string, parentId: number | null, isCommon?: boolean): Observable<unknown> {
    return this.http.patch(`${this.base}/expense-categories/${id}`, { name, parentId, isCommon });
  }

  deleteExpenseCategory(id: number): Observable<unknown> {
    return this.http.delete(`${this.base}/expense-categories/${id}`);
  }

  getIncomeCategories(): Observable<Category[]> {
    // El backend las devuelve planas (con parentId); construimos el árbol y aplanamos con sangría.
    return this.http
      .get<Category[]>(`${this.base}/income-categories`)
      .pipe(map((flat) => this.buildTreeAndFlatten(flat)));
  }

  /** Categorías de gasto: el backend las devuelve en árbol; las aplanamos para el desplegable. */
  getExpenseCategories(): Observable<Category[]> {
    return this.http
      .get<Category[]>(`${this.base}/expense-categories`)
      .pipe(map((tree) => this.flatten(tree)));
  }

  /** Construye jerarquía a partir de una lista plana (con parentId) y la aplana con sangría. */
  private buildTreeAndFlatten(flat: Category[]): Category[] {
    const childrenOf = new Map<number | null, Category[]>();
    for (const c of flat) {
      const p = c.parentId ?? null;
      if (!childrenOf.has(p)) childrenOf.set(p, []);
      childrenOf.get(p)!.push(c);
    }
    const out: Category[] = [];
    const walk = (parent: number | null, depth: number): void => {
      for (const c of childrenOf.get(parent) ?? []) {
        out.push({
          id: c.id,
          name: c.name,
          label: `${'— '.repeat(depth)}${c.name}`,
          parentId: c.parentId ?? null,
          userId: c.userId ?? null,
          isCommon: c.isCommon ?? c.userId == null,
        });
        walk(c.id, depth + 1);
      }
    };
    walk(null, 0);
    return out;
  }

  private flatten(nodes: Category[], depth = 0): Category[] {
    const out: Category[] = [];
    for (const n of nodes ?? []) {
      out.push({
        id: n.id,
        name: n.name,
        label: `${'— '.repeat(depth)}${n.name}`,
        parentId: n.parentId ?? null,
        userId: n.userId ?? null,
        isCommon: n.isCommon ?? n.userId == null,
      });
      if (n.children?.length) {
        out.push(...this.flatten(n.children, depth + 1));
      }
    }
    return out;
  }
}
