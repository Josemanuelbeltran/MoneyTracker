import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="flex min-h-screen items-center justify-center bg-bm-bg px-4">
      <div class="w-full max-w-sm">
        <div class="mb-6 text-center">
          <div class="mx-auto mb-3 flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-to-br from-bm-cyan to-cyan-700 text-lg font-bold text-bm-bg shadow-glow">
            MT
          </div>
          <h1 class="text-xl font-semibold text-bm-text">MoneyTracker</h1>
          <p class="text-sm text-bm-muted">Controla tus ingresos y gastos</p>
        </div>

        <div class="mt-card p-6">
          <h2 class="mb-5 text-center text-base font-semibold text-bm-text">Iniciar sesión</h2>

          <label class="mt-label">Usuario</label>
          <input class="mt-input mb-4" type="text" placeholder="tu usuario"
                 [(ngModel)]="username" (keyup.enter)="login()" />

          <label class="mt-label">Contraseña</label>
          <input class="mt-input mb-5" type="password" placeholder="••••••••"
                 [(ngModel)]="password" (keyup.enter)="login()" />

          <button class="mt-btn-cyan w-full" (click)="login()" [disabled]="loading">
            <i class="pi pi-sign-in"></i> Entrar
          </button>
        </div>

        <p class="mt-4 text-center text-sm text-bm-muted">
          ¿No tienes cuenta?
          <a routerLink="/register" class="font-semibold text-bm-cyan hover:underline">Crear cuenta</a>
        </p>
      </div>
    </div>
  `,
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  private msg = inject(MessageService);

  username = '';
  password = '';
  loading = false;

  login(): void {
    if (!this.username || !this.password) {
      this.msg.add({ severity: 'warn', summary: 'Faltan datos', detail: 'Introduce usuario y contraseña' });
      return;
    }
    this.loading = true;
    this.auth.login(this.username, this.password).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigateByUrl('/');
      },
      error: () => {
        this.loading = false;
        this.msg.add({ severity: 'error', summary: 'Error', detail: 'Credenciales incorrectas' });
      },
    });
  }
}
