import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

const TOKEN_KEY = 'mt_token';
const USER_KEY = 'mt_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  /** Nombre del usuario logueado (para el saludo). */
  readonly userName = signal<string>(localStorage.getItem(USER_KEY) || '');

  /** Login con usuario (nombre) + contraseña. */
  login(username: string, password: string): Observable<{ token: string }> {
    return this.http
      .post<{ token: string }>(`${this.base}/auth/login`, { name: username, passwordHash: password })
      .pipe(
        tap((res) => {
          localStorage.setItem(TOKEN_KEY, res.token);
          localStorage.setItem(USER_KEY, username);
          this.userName.set(username);
        }),
      );
  }

  register(name: string, email: string, password: string): Observable<unknown> {
    return this.http.post(`${this.base}/users`, {
      name,
      email,
      passwordHash: password,
      verifiedPhone: false,
    });
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.userName.set('');
  }

  get token(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  /** Hay sesión válida solo si existe token y NO está caducado. */
  isAuthenticated(): boolean {
    const token = this.token;
    if (!token) return false;
    const exp = this.tokenExp(token);
    if (exp === null) return true; // token no estándar: dejar pasar (el 401 del interceptor hará de red).
    return Date.now() < exp * 1000;
  }

  /**
   * ¿El usuario es administrador? (claim `is_admin` del JWT). El admin es quien decide
   * qué categorías son comunes para todos; el backend vuelve a comprobarlo siempre.
   */
  isAdmin(): boolean {
    const payload = this.tokenPayload(this.token);
    return payload?.is_admin === 'true' || payload?.is_admin === true;
  }

  /** Lee el claim `exp` (segundos epoch) del payload del JWT, o null si no se puede. */
  private tokenExp(token: string): number | null {
    const exp = this.tokenPayload(token)?.exp;
    return typeof exp === 'number' ? exp : null;
  }

  /** Payload del JWT ya decodificado, o null si el token no es legible. */
  private tokenPayload(token: string | null): { exp?: number; is_admin?: string | boolean } | null {
    if (!token) return null;
    try {
      const payload = token.split('.')[1];
      if (!payload) return null;
      const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(json) as { exp?: number; is_admin?: string | boolean };
    } catch {
      return null;
    }
  }
}
