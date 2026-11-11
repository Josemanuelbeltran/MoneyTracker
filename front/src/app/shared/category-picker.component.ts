import { Component, computed, input, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Category } from '../core/models/category.model';

interface Level {
  options: Category[];
  selected: number | null;
}

/**
 * Selector de categoría en cascada: primero muestra las categorías padre;
 * al elegir una que tiene subcategorías, aparece otro desplegable para seguir
 * bajando. El valor es la categoría más profunda seleccionada (puedes quedarte
 * en la padre si no eliges subcategoría).
 * Recibe la lista PLANA de categorías (con parentId) y expone [(value)].
 */
@Component({
  selector: 'app-category-picker',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-2">
      <select
        *ngFor="let lvl of levels(); let i = index"
        class="mt-input"
        [ngModel]="lvl.selected"
        (ngModelChange)="onSelect(i, $event)">
        <option [ngValue]="null">{{ i === 0 ? 'Selecciona categoría' : 'Subcategoría (opcional)' }}</option>
        <option *ngFor="let c of lvl.options" [ngValue]="c.id">{{ c.name }}</option>
      </select>
    </div>
  `,
})
export class CategoryPickerComponent {
  categories = input<Category[]>([]);
  value = model<number | null>(null);

  private childrenOf(parentId: number | null): Category[] {
    return this.categories().filter((c) => (c.parentId ?? null) === parentId);
  }

  /** Ruta de ids desde la raíz hasta el valor seleccionado (incluido). */
  private path = computed<number[]>(() => {
    const v = this.value();
    if (v == null) return [];
    const byId = new Map(this.categories().map((c) => [c.id, c]));
    const chain: number[] = [];
    let cur: number | null | undefined = v;
    const seen = new Set<number>();
    while (cur != null && byId.has(cur) && !seen.has(cur)) {
      seen.add(cur);
      chain.unshift(cur);
      cur = byId.get(cur)!.parentId ?? null;
    }
    return chain;
  });

  levels = computed<Level[]>(() => {
    const path = this.path();
    const levels: Level[] = [{ options: this.childrenOf(null), selected: path[0] ?? null }];
    for (let i = 0; i < path.length; i++) {
      const kids = this.childrenOf(path[i]);
      if (kids.length) levels.push({ options: kids, selected: path[i + 1] ?? null });
    }
    return levels;
  });

  onSelect(levelIndex: number, newId: number | null): void {
    if (newId != null) {
      this.value.set(newId);
      return;
    }
    // Deseleccionar en este nivel: subir al nivel anterior (o vaciar en el nivel 0).
    const path = this.path();
    this.value.set(levelIndex === 0 ? null : path[levelIndex - 1] ?? null);
  }
}
