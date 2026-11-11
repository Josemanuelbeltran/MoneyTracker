# CLAUDE.md

Orientación para Claude Code al trabajar con el **frontend** de MoneyTracker.

## Stack (igual que el panel BM2 de Printer3D)

- **Angular 20** (standalone components, `app.routes.ts`, `bootstrapApplication`)
- **Tailwind CSS 3** (paleta `bm` en `tailwind.config.js`)
- **PrimeNG 20** + `@primeuix/themes` (preset `Bm2Preset`) + **primeicons**
- **TypeScript**, **RxJS**, signals
- NO usa Ionic ni Capacitor (se descartó; se rehízo para clonar el look de BM2)

## Comandos

```bash
npm install
npm start        # ng serve → http://localhost:4300
npm run build    # ng build
```

## Estructura (`src/app/`)

- **core/** — infraestructura: `services/` (auth, finance), `guards/` (authGuard), `interceptors/` (authInterceptor JWT), `models/`, `theme/bm2-preset.ts`
- **layout/shell/** — layout con barra lateral colapsable + topbar (clon del shell de BM2)
- **pages/** — login, register, dashboard, incomes, expenses

## Integración con el backend

- API en `../back` (ASP.NET Core), URL en `environments/environment.ts` → `apiUrl: http://localhost:5138`.
- **Login por usuario (nombre) + contraseña** (no email): `POST /auth/login { name, passwordHash }`.
- **JWT**: el `authInterceptor` adjunta `Authorization: Bearer <token>` leyendo `localStorage('mt_token')`.
- CORS: el backend permite cualquier origen localhost en desarrollo.
- Los modelos (`core/models/`) reflejan los DTOs del backend; mantener sincronizados.

## Convenciones

- Estilo visual con la paleta `bm` (fondo `#0F1419`, sidebar `#161B22`, superficie `#1C2128`, cian `#2EC5FF`, success `#3FB950`, danger `#F85149`).
- Clases de componente reutilizables en `styles.scss`: `.mt-card`, `.mt-input`, `.mt-label`, `.mt-btn-cyan`, `.mt-btn-ghost`.
- Avisos al usuario con **PrimeNG `MessageService`** (toast global en `app.component`).
- Sin lógica de negocio en las páginas: delegar en servicios de `core/`.
- Rutas en `app.routes.ts`; privadas protegidas con `authGuard`.

## Pendiente

- Al añadir movimientos aún se teclean los `id` de cuenta y categoría → faltan endpoints de listado (cuentas, categorías de ingreso) para poner desplegables.
- Editar movimientos desde la UI; páginas de cuentas y categorías.
