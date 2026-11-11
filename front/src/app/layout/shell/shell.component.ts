import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TooltipModule } from 'primeng/tooltip';
import { AuthService } from '../../core/services/auth.service';
import { FinanceService } from '../../core/services/finance.service';

interface NavItem {
  label: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet, TooltipModule],
  template: `
    <div class="flex h-[100dvh] w-full overflow-hidden bg-bm-bg text-bm-text">
      <!-- Backdrop (solo móvil, cuando el cajón está abierto) -->
      <div *ngIf="mobileOpen()" (click)="mobileOpen.set(false)"
           class="fixed inset-0 z-30 bg-black/60 lg:hidden"></div>

      <!-- Sidebar: estático en escritorio, cajón deslizante en móvil -->
      <aside
        class="fixed inset-y-0 left-0 z-40 flex w-60 flex-col border-r border-bm-border bg-bm-sidebar transition-transform duration-200 lg:static lg:z-auto lg:translate-x-0"
        [class.-translate-x-full]="!mobileOpen()"
        [class.lg:w-60]="!collapsed()"
        [class.lg:w-16]="collapsed()">
        <!-- Logo -->
        <div class="flex h-16 items-center border-b border-bm-border px-4"
             [class.lg:justify-center]="collapsed()"
             [class.lg:px-0]="collapsed()">
          <div class="flex items-center gap-2 overflow-hidden">
            <div class="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-gradient-to-br from-bm-cyan to-cyan-700 text-xs font-bold text-bm-bg"
                 [class.shadow-glow]="!collapsed()">
              MT
            </div>
            <span class="truncate text-sm font-semibold tracking-wider" [class.lg:hidden]="collapsed()">
              MONEYTRACKER
            </span>
          </div>
        </div>

        <!-- Nav -->
        <nav class="flex-1 overflow-y-auto p-3">
          <ul class="space-y-1">
            <li *ngFor="let item of nav">
              <a
                [routerLink]="item.path"
                (click)="mobileOpen.set(false)"
                routerLinkActive="bg-bm-surface-2 !text-bm-cyan"
                class="group flex items-center gap-3 rounded-lg border border-transparent px-3 py-2 text-sm text-bm-muted transition-colors hover:bg-bm-surface hover:text-bm-text"
                [pTooltip]="collapsed() ? item.label : ''"
                tooltipPosition="right">
                <i class="pi {{ item.icon }} text-base"></i>
                <span class="truncate" [class.lg:hidden]="collapsed()">{{ item.label }}</span>
              </a>
            </li>
          </ul>
        </nav>

        <!-- Footer / user -->
        <div class="border-t border-bm-border p-3">
          <div class="flex items-center gap-3" [class.lg:justify-center]="collapsed()">
            <div class="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-bm-surface-2 text-xs font-semibold text-bm-cyan">
              {{ initials() }}
            </div>
            <div class="flex-1 overflow-hidden" [class.lg:hidden]="collapsed()">
              <div class="truncate text-sm font-medium text-bm-text">{{ userName() || 'Usuario' }}</div>
              <button (click)="logout()" class="text-xs text-bm-muted transition-colors hover:text-bm-cyan">
                Cerrar sesión
              </button>
            </div>
          </div>
        </div>
      </aside>

      <!-- Main column -->
      <div class="flex flex-1 flex-col overflow-hidden">
        <!-- Topbar -->
        <header class="flex h-16 shrink-0 items-center gap-4 border-b border-bm-border bg-bm-bg/80 px-4 backdrop-blur sm:px-6">
          <!-- Botón menú: abre el cajón en móvil -->
          <button
            (click)="mobileOpen.set(true)"
            class="flex h-9 w-9 items-center justify-center rounded-lg text-bm-muted transition-colors hover:bg-bm-surface hover:text-bm-text lg:hidden">
            <i class="pi pi-bars"></i>
          </button>
          <!-- Botón menú: colapsa/expande en escritorio -->
          <button
            (click)="toggleSidebar()"
            class="hidden h-9 w-9 items-center justify-center rounded-lg text-bm-muted transition-colors hover:bg-bm-surface hover:text-bm-text lg:flex">
            <i class="pi pi-bars"></i>
          </button>
          <span class="text-sm font-semibold tracking-wider text-bm-text lg:hidden">MONEYTRACKER</span>
          <div class="flex-1"></div>
          <div class="flex items-center gap-2">
            <span class="hidden text-xs text-bm-muted sm:inline">v1.0</span>
            <span class="h-2 w-2 rounded-full bg-bm-success shadow-[0_0_8px_rgba(63,185,80,0.6)]"></span>
          </div>
        </header>

        <!-- Content -->
        <main class="min-h-0 flex-1 overflow-y-auto overscroll-contain p-4 sm:p-6">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `,
})
export class ShellComponent implements OnInit {
  private auth = inject(AuthService);
  private router = inject(Router);
  private finance = inject(FinanceService);

  ngOnInit(): void {
    // Al entrar en la app, generar los movimientos fijos que falten este mes.
    this.finance.generateRecurring().subscribe();
  }

  collapsed = signal(false); // colapsado en escritorio
  mobileOpen = signal(false); // cajón abierto en móvil
  userName = this.auth.userName;
  initials = computed(() => {
    const name = this.userName() || '?';
    return name.split(' ').map((p) => p[0]).slice(0, 2).join('').toUpperCase();
  });

  nav: NavItem[] = [
    { label: 'Resumen', icon: 'pi-home', path: '/dashboard' },
    { label: 'Ingresos', icon: 'pi-arrow-up-right', path: '/incomes' },
    { label: 'Gastos', icon: 'pi-arrow-down-left', path: '/expenses' },
    { label: 'Cobros', icon: 'pi-clock', path: '/pending' },
    { label: 'Cuentas', icon: 'pi-wallet', path: '/accounts' },
    { label: 'Presupuestos', icon: 'pi-chart-bar', path: '/budgets' },
    { label: 'Estadísticas', icon: 'pi-chart-line', path: '/stats' },
    { label: 'Ajustes', icon: 'pi-cog', path: '/settings' },
  ];

  toggleSidebar(): void {
    this.collapsed.update((v) => !v);
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
