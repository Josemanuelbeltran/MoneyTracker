# MoneyTracker

Aplicación web full-stack de finanzas personales, en producción en **[moneytracker.es](https://moneytracker.es)**.

## Funcionalidades

- Cuentas, ingresos, gastos y traspasos entre cuentas
- **Sobres** (pockets) para repartir el dinero y **presupuestos** por categoría
- **Gastos compartidos** con reparto configurable (p. ej. 70/30), prorrateo en varios meses y seguimiento de reembolsos y cobros pendientes por persona
- Movimientos **recurrentes**, estadísticas y dashboard
- Categorías comunes y propias, con rol de administrador
- Integración con una API externa (PetRecording) para sincronizar los gastos de mascotas

## Stack

| Capa | Tecnología |
|------|------------|
| Backend | .NET 9, ASP.NET Core Minimal API |
| Arquitectura | Hexagonal / Clean Architecture, CQRS ligero, casos de uso |
| Persistencia | EF Core 9 + MySQL / MariaDB, migraciones versionadas con DbUp |
| Seguridad | JWT, contraseñas con BCrypt, recursos filtrados por usuario |
| Frontend | Angular 20, Tailwind CSS, PrimeNG |
| CI/CD | Azure Pipelines con agente self-hosted, despliegue como servicio systemd en Linux |
| Operación | Cloudflare Tunnel (HTTPS), backups diarios automáticos con rotación |

## Arquitectura del backend

```
back/
├── MoneyTracker.Domain                          Entidades, contratos de casos de uso, puertos (IRepository / IQueryService)
├── MoneyTracker.Application                     Implementación de los casos de uso
├── MoneyTracker.Infrastructure.MySql            DbContext, DTOs, mappers y migraciones SQL
├── MoneyTracker.Infrastructure.MySql.QueryService   Lectura (EF Core + AsNoTracking)
├── MoneyTracker.Infrastructure.MySql.Repository     Escritura (EF Core)
└── MoneyTracker.Host.Api                        Endpoints Minimal API, JWT, manejo global de errores
```

- El dominio no depende de ningún framework; la infraestructura implementa sus interfaces.
- Los endpoints solo llaman a interfaces de casos de uso (`IXxxUseCase`), nunca a repositorios directamente.
- Lectura y escritura separadas: `QueryService` para consultar y `Repository` para modificar.

## Ejecutar en local

Requisitos: .NET 9 SDK, Node 20 y MySQL 8 (o MariaDB).

1. Crea una base de datos `MoneyTracker` y ajusta `ConnectionStrings:DefaultConnection` y `Jwt:Key` en `back/MoneyTracker.Host.Api/appsettings.json`. Las tablas las crea DbUp al arrancar.
2. Backend:
   ```bash
   cd back
   dotnet run --project MoneyTracker.Host.Api
   ```
3. Frontend:
   ```bash
   cd front
   npm install
   npm start
   ```
