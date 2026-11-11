import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Adjunta el token JWT (Bearer) a cada petición y maneja los 401/403:
 * si el token está caducado o es inválido, cierra sesión y redirige a /login
 * (en vez de dejar la app "vacía" con un token muerto).
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = localStorage.getItem('mt_token');
  if (token) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse && (err.status === 401 || err.status === 403)) {
        // No redirigir en el propio login: ahí el 401 significa "credenciales incorrectas".
        const isLoginCall = req.url.includes('/auth/login');
        if (!isLoginCall) {
          auth.logout();
          router.navigate(['/login']);
        }
      }
      return throwError(() => err);
    }),
  );
};
