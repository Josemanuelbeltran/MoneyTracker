import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="flex min-h-screen items-center justify-center bg-bm-bg px-4 py-8">
      <div class="w-full max-w-sm">
        <div class="mb-6 text-center">
          <div class="mx-auto mb-3 flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-to-br from-bm-cyan to-cyan-700 text-lg font-bold text-bm-bg shadow-glow">
            MT
          </div>
          <h1 class="text-xl font-semibold text-bm-text">Crear cuenta</h1>
          <p class="text-sm text-bm-muted">Empieza a controlar tu dinero</p>
        </div>

        <div class="mt-card p-6">
          <label class="mt-label">Usuario</label>
          <input class="mt-input mb-4" type="text" placeholder="nombre de usuario" [(ngModel)]="name" />

          <label class="mt-label">Email</label>
          <input class="mt-input mb-4" type="email" placeholder="tucorreo@email.com" [(ngModel)]="email" />

          <label class="mt-label">Contraseña</label>
          <input class="mt-input mb-4" type="password" placeholder="••••••••" [(ngModel)]="password" />

          <label class="mt-label">Repite la contraseña</label>
          <input class="mt-input mb-5" type="password" placeholder="••••••••" [(ngModel)]="confirm" />

          <button class="mt-btn-cyan w-full" (click)="register()" [disabled]="loading">
            <i class="pi pi-user-plus"></i> Crear cuenta
          </button>
        </div>

        <p class="mt-4 text-center text-sm text-bm-muted">
          ¿Ya tienes cuenta?
          <a routerLink="/login" class="font-semibold text-bm-cyan hover:underline">Iniciar sesión</a>
        </p>
      </div>
    </div>
  `,
})
export class RegisterComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  private msg = inject(MessageService);

  name = '';
  email = '';
  password = '';
  confirm = '';
  loading = false;

  register(): void {
    if (!this.name || !this.email || !this.password) {
      this.msg.add({ severity: 'warn', summary: 'Faltan datos', detail: 'Rellena usuario, email y contraseña' });
      return;
    }
    if (this.password !== this.confirm) {
      this.msg.add({ severity: 'warn', summary: 'Contraseñas', detail: 'Las contraseñas no coinciden' });
      return;
    }
    this.loading = true;
    this.auth.register(this.name, this.email, this.password).subscribe({
      next: () => {
        this.loading = false;
        this.msg.add({ severity: 'success', summary: 'Listo', detail: 'Cuenta creada, ya puedes entrar' });
        this.router.navigateByUrl('/login');
      },
      error: () => {
        this.loading = false;
        this.msg.add({ severity: 'error', summary: 'Error', detail: 'No se pudo crear (¿usuario/email ya existe?)' });
      },
    });
  }
}
