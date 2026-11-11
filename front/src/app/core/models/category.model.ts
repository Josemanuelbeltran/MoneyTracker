export interface Category {
  id: number;
  name: string;
  label?: string; // nombre con sangría para mostrar en árbol
  parentId?: number | null;
  children?: Category[];
  /** Dueño de la categoría. null = categoría común del sistema. */
  userId?: number | null;
  /** true = común (la ven todos; solo un admin la edita o la borra). */
  isCommon?: boolean;
}
