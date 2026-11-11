import { Component, OnInit, computed, model, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface Cursor {
  i: number;
  s: string;
}

/**
 * Campo de importe con calculadora: se puede escribir una expresión (p.ej. "25+25")
 * y se evalúa al vuelo. Incluye un teclado opcional (botón calculadora).
 * Evaluación segura mediante un parser propio (sin eval).
 */
@Component({
  selector: 'app-calc-amount',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div>
      <div class="flex gap-2">
        <input
          class="mt-input flex-1"
          type="text"
          inputmode="decimal"
          [ngModel]="expr()"
          (ngModelChange)="onExpr($event)"
          placeholder="0  (admite 25+25)" />
        <button type="button" class="mt-btn-ghost px-3" (click)="showPad.set(!showPad())" title="Calculadora">
          <i class="pi pi-calculator"></i>
        </button>
      </div>

      <div *ngIf="preview() !== null" class="mt-1 text-xs text-bm-muted">= {{ preview() | number: '1.2-2' }} €</div>

      <div *ngIf="showPad()" class="mt-2 grid grid-cols-4 gap-1.5">
        <button *ngFor="let k of keys" type="button" (click)="press(k)"
                class="rounded-lg border border-bm-border py-2 text-sm text-bm-text transition-colors hover:bg-bm-surface-2"
                [class.text-bm-cyan]="k === '='">
          {{ k }}
        </button>
      </div>
    </div>
  `,
})
export class CalcAmountComponent implements OnInit {
  /** Valor numérico resultante (two-way: [(value)]). */
  value = model<number | null>(null);

  expr = signal<string>('');
  showPad = signal(false);

  ngOnInit(): void {
    // Precarga el campo con el valor inicial (p.ej. al editar).
    const v = this.value();
    if (v != null) this.expr.set(String(v));
  }

  readonly keys = ['C', '⌫', '(', ')', '7', '8', '9', '÷', '4', '5', '6', '×', '1', '2', '3', '−', '0', '.', '+', '='];

  /** Vista previa del resultado solo cuando hay una operación de por medio. */
  preview = computed<number | null>(() => {
    const e = this.expr();
    if (!/[-+*/×÷]/.test(e.slice(1))) return null;
    return this.evaluate(e);
  });

  onExpr(v: string): void {
    this.expr.set(v);
    this.recompute();
  }

  press(k: string): void {
    if (k === 'C') {
      this.expr.set('');
    } else if (k === '⌫') {
      this.expr.set(this.expr().slice(0, -1));
    } else if (k === '=') {
      const r = this.evaluate(this.expr());
      if (r !== null) this.expr.set(String(r));
    } else {
      this.expr.set(this.expr() + k);
    }
    this.recompute();
  }

  private recompute(): void {
    this.value.set(this.evaluate(this.expr()));
  }

  /** Evalúa una expresión aritmética simple. Devuelve null si no es válida. */
  private evaluate(raw: string): number | null {
    const s = raw
      .replace(/\s+/g, '')
      .replace(/,/g, '.')
      .replace(/×/g, '*')
      .replace(/÷/g, '/')
      .replace(/−/g, '-');
    if (!s) return null;
    if (!/^[-+*/().0-9]+$/.test(s)) return null;

    const c: Cursor = { i: 0, s };
    try {
      const v = this.parseExpression(c);
      if (c.i !== s.length) return null; // expresión incompleta o con basura
      if (!isFinite(v)) return null;
      return Math.round(v * 100) / 100;
    } catch {
      return null;
    }
  }

  private parseExpression(c: Cursor): number {
    let v = this.parseTerm(c);
    while (c.i < c.s.length && (c.s[c.i] === '+' || c.s[c.i] === '-')) {
      const op = c.s[c.i++];
      const r = this.parseTerm(c);
      v = op === '+' ? v + r : v - r;
    }
    return v;
  }

  private parseTerm(c: Cursor): number {
    let v = this.parseFactor(c);
    while (c.i < c.s.length && (c.s[c.i] === '*' || c.s[c.i] === '/')) {
      const op = c.s[c.i++];
      const r = this.parseFactor(c);
      v = op === '*' ? v * r : v / r;
    }
    return v;
  }

  private parseFactor(c: Cursor): number {
    if (c.s[c.i] === '+') {
      c.i++;
      return this.parseFactor(c);
    }
    if (c.s[c.i] === '-') {
      c.i++;
      return -this.parseFactor(c);
    }
    if (c.s[c.i] === '(') {
      c.i++;
      const v = this.parseExpression(c);
      if (c.s[c.i] !== ')') throw new Error('paren');
      c.i++;
      return v;
    }
    const start = c.i;
    while (c.i < c.s.length && /[0-9.]/.test(c.s[c.i])) c.i++;
    if (c.i === start) throw new Error('number');
    const num = parseFloat(c.s.slice(start, c.i));
    if (isNaN(num)) throw new Error('nan');
    return num;
  }
}
