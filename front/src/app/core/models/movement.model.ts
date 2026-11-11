export interface Income {
  id: number;
  userId: number;
  accountId: number;
  categoryId: number;
  amount: number;
  incomeDate: string;
  description?: string;
  isRecurring?: boolean;
}

export interface Expense {
  id: number;
  userId: number;
  accountId: number;
  categoryId: number;
  amount: number;
  expenseDate: string;
  description?: string;
  isRecurring?: boolean;
  pocketId?: number | null;
  /** Mascota a la que va el gasto (id de PetRecording). NULL = no es de mascota. */
  petId?: number | null;
  /** Importe completo del gasto compartido (null si no es parcial). */
  totalAmount?: number | null;
  /** Entre cuántas partes se divide (null si no es parcial). */
  splitParts?: number | null;
  /** Id común a las cuotas de un mismo prorrateo (null si no está prorrateado). */
  spreadGroup?: string | null;
  /** Nº de cuota dentro del prorrateo, empezando en 1. */
  spreadIndex?: number | null;
  /** Total de cuotas del prorrateo. */
  spreadCount?: number | null;
}

/**
 * Parte de un gasto compartido que adelantaste y te tienen que devolver
 * (el bizum). Mientras `settledAt` es null el dinero sigue fuera de tu cuenta.
 */
export interface Reimbursement {
  id: number;
  userId: number;
  expenseId: number;
  amount: number;
  /** Cuenta donde te harán el bizum. */
  toAccountId: number;
  /** null = pendiente de cobro; con fecha = ya cobrado. */
  settledAt?: string | null;
  /** Persona de tu agenda que te lo debe. null si no la indicaste. */
  personId?: number | null;
  /** Quién te lo debe. Texto libre. */
  debtor?: string | null;
}
