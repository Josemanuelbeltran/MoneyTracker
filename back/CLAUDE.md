# CLAUDE.md

Este archivo proporciona orientación a Claude Code (claude.ai/code) al trabajar con el **backend** de MoneyTracker.

## Comandos de Compilación y Ejecución

```bash
# Compilar la solución (desde back/)
dotnet build MoneyTracker.sln

# Ejecutar la API
dotnet run --project MoneyTracker.Host.Api

# Compilar para producción
dotnet publish MoneyTracker.Host.Api -c Release
```

## Visión General de la Arquitectura

Backend **.NET 9** (ASP.NET Core) con **Arquitectura Hexagonal / Clean Architecture** para gestión de finanzas personales (cuentas, ingresos, gastos y categorías). Sigue patrones **CQRS ligero** con diseño orientado a casos de uso.

**CQRS Ligero** (a diferencia de otros proyectos, aquí **NO se usa Dapper**):

- **Lectura**: `QueryService` sobre `AppDbContext` (EF Core) con `AsNoTracking()`
- **Escritura**: `Repository` sobre `AppDbContext` (EF Core)
- No separamos los modelos de lectura/escritura: se usan las mismas entidades de dominio (`XxxEntity`)

### Stack

- **.NET 9** / ASP.NET Core (Minimal API mapeada vía extension methods)
- **EF Core 9** + `Pomelo.EntityFrameworkCore.MySql` (MySQL)
- **Autenticación**: JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`) + `System.IdentityModel.Tokens.Jwt`
- **Passwords**: `BCrypt.Net-Next`
- **OpenAPI**: `Microsoft.AspNetCore.OpenApi`
- Sin RabbitMQ, sin cache distribuida, sin paquetes internos Artifact/Deneva

### Estructura de Proyectos

- **MoneyTracker.Domain**: Entidades (`XxxEntity`), interfaces de `IRepository` / `IQueryService`, contratos de casos de uso (`Application/{Feature}/I{Nombre}UseCase.cs`) e interfaces de seguridad (`IJwtTokenGenerator`). Sin dependencias de frameworks.
- **MoneyTracker.Application**: Implementaciones de casos de uso en `UseCases/{Feature}/{Nombre}UseCase.cs` y seguridad (`JwtTokenGenerator`). Registro de DI en `ServiceExtension.cs` (`AddApplicationService`).
- **MoneyTracker.Infrastructure.MySql**: `AppDbContext` (carpeta `DDBBConection`), DTOs de persistencia (`Dtos/`), mapeadores (`Mappers/` con `ToDomain()` / `ToEntity()`) y scripts (`Scripts/`). Registro `AddMySqlPersistence`.
- **MoneyTracker.Infrastructure.MySql.QueryService**: Acceso de **lectura** vía `AppDbContext` + `AsNoTracking()`. Registro `AddQueryService`.
- **MoneyTracker.Infrastructure.MySql.Repository**: Acceso de **escritura** vía EF Core. Registro `AddRepositoryService`.
- **MoneyTracker.Host.Api**: Punto de entrada. Endpoints en `Controllers/Map{Entidad}ControllerExtension.cs`, configuración en `Configurations/` (`ApiExtension.MapApi()`, `JwtServiceExtension`), respuesta uniforme en `Response/HttpResponse<T>`.

***

## Capa de Domain

Núcleo de la aplicación: entidades de negocio, interfaces de `Repository`/`QueryService` y contratos de casos de uso. Sin dependencias externas ni frameworks.

**Convenciones rápidas:**
- Entidades con sufijo `Entity` en `Domain/Entities/` (subcarpetas `Categories/`, `types/` para agrupar).
- Interfaces de casos de uso en `Domain/Application/{Feature}/I{Nombre}UseCase.cs`.
- Interfaces de repositorio en `Domain/Repository/`, de lectura en `Domain/QueryService/`.

***

## Capa de Application

Orquesta los casos de uso coordinando repositorios y query services. Cada caso de uso = una acción de negocio, con método `ExecuteAsync(...)`.

**Convenciones rápidas:**
- Implementación en `Application/UseCases/{Feature}/{Nombre}UseCase.cs`, implementa la interfaz del Domain.
- Todos los métodos `async`.
- Registrar como `Scoped` en `Application/ServiceExtension.cs` (`AddApplicationService`), agrupado por feature con comentario (`//Account`, `//Expense`…).
- `IJwtTokenGenerator` se registra como `Singleton`.

> ⚠️ **Deuda técnica conocida**: el namespace de este proyecto es `ClassLibrary1MoneyTracker.Application` (nombre por defecto sin renombrar). No introducir nuevos usings apoyándose en él sin necesidad; si se renombra, hacerlo de forma global.

***

## Capa de Infrastructure

Implementaciones concretas de los puertos del dominio. Nomenclatura: `Infrastructure.MySql[.{Tipo}]`.

**QueryService (lectura / EF Core):**
- Inyecta `AppDbContext`, consulta con `AsNoTracking()` y mapea a dominio con `.ToDomain()`.
- Implementa `I{Nombre}QueryService` del Domain.
- Registrar en `QueryService/ServiceExtension.cs` (`AddQueryService`).

**Repository (escritura / EF Core):**
- `Infrastructure.MySql.Repository/{Entidad}/{Entidad}Repository.cs`, implementa `I{Nombre}Repository`.
- **Los repositorios reciben entidades de dominio** (`XxxEntity`); el mapeo `ToEntity()` desde el DTO se hace en el controller, no en el repositorio.
- Registrar en `Repository/ServiceExtension.cs` (`AddRepositoryService`).
- ⚠️ Ante cualquier cambio de esquema en BD: extraer el SQL y **pedir confirmación** antes de ejecutar.

**DbContext / Mapeo:**
- `AppDbContext` en `Infrastructure.MySql/DDBBConection/`.
- DTOs de persistencia en `Infrastructure.MySql/Dtos/`, mapeadores en `Infrastructure.MySql/Mappers/` (`ToDomain()` / `ToEntity()`).

***

## Capa de Host (Host.Api)

Punto de entrada ASP.NET Core: expone endpoints REST y gestiona auth, errores y DI.

**Convenciones rápidas:**
- Un archivo por entidad: `Host.Api/Controllers/Map{Entidad}ControllerExtension.cs`, clase estática con un extension method `Add{Entidad}Controllers(this WebApplication app)`.
- Dentro se definen los endpoints con `app.MapGet/MapPost/MapPatch/MapDelete(...)` y `.RequireAuthorization()` donde aplique.
- Registrar cada grupo en `Configurations/ApiExtension.cs` → `MapApi()`.
- **Llamar siempre a las interfaces de casos de uso** (`IXxxUseCase`), nunca a repositorios/query services directamente.
- Construir la entidad de dominio a partir del DTO del body **en el endpoint** antes de llamar al caso de uso.
- El `userId` se obtiene del claim `sub` del JWT (`JwtRegisteredClaimNames.Sub` / `ClaimTypes.NameIdentifier`).

**Manejo de errores:**
- Middleware global en `ApiExtension.MapApi()` (`UseExceptionHandler`): mapea excepciones a códigos HTTP y devuelve `Response.HttpResponse<T>.Fail(...)`.
  - `UnauthorizedAccessException` → 401 · `ValidationException`/`ArgumentException` → 400 · `KeyNotFoundException` → 404 · resto → 500.

***

## Patrones Clave

### Casos de Uso
- Interfaz en `Domain/Application/{Feature}/I{Nombre}UseCase.cs`, implementación en `Application/UseCases/{Feature}/{Nombre}UseCase.cs`, método `ExecuteAsync(...)`.
- Registrados como `Scoped` en `Application/ServiceExtension.cs`.
- **Siempre llamar a las interfaces de casos de uso** desde los endpoints.

### Lectura vs Escritura
- **Lectura**: `QueryService` con `AppDbContext` + `AsNoTracking()`.
- **Escritura**: `Repository` con `AppDbContext`.
- Ambos con EF Core (aquí no hay Dapper).

### Configuración
- Cadena de conexión: `ConnectionStrings:DefaultConnection` en `appsettings.json` (obligatoria; `AddMySqlPersistence` lanza excepción si falta).
- JWT configurado en `Configurations/JwtServiceExtension.cs` (`AddJwtAuthentication`).

***

## Convenciones Generales de Código

### Nomenclatura
- **Clases, métodos y propiedades**: PascalCase · **parámetros y variables locales**: camelCase.
- **Dominio**: entidades con sufijo `Entity` (`AccountEntity`, `ExpenseEntity`…).
- **Application / Infrastructure**: modelos de transporte con sufijo `Dto` / `Request` / `Response` (`AccountDto`, `AccountPathDto`…). Nunca exponer `Entity` como body de request.
- **Sufijos obligatorios**: `Entity`, `UseCase`, `Repository`, `QueryService`, `ControllerExtension`.

### Prácticas de Código
- **Inyección de dependencias**: siempre por interfaz, nunca implementación concreta.
- **Async/Await**: todos los métodos de UseCase, Repository y QueryService son `async` (`ExecuteAsync`, `...Async`).
- **Repositorios reciben entidades**: el mapeo `DTO → Entity` (`ToEntity()`) se hace en el controller/endpoint.

***

## Principios de Diseño

- **Inversión de dependencias**: Domain define contratos (interfaces), Infrastructure los implementa.
- **Siempre llamar a interfaces de casos de uso** en los endpoints, nunca directamente a repositorios o query services.
- **Separación lectura/escritura (CQRS ligero)**: QueryService para lectura, Repository para escritura (ambos EF Core).
- **Single Responsibility**: un caso de uso = una acción del negocio.
- **⚠️ Cambios en BD**: siempre extraer el SQL y pedir confirmación antes de ejecutar.

***

## Checklist de Desarrollo (feature completo)

- [ ] **Domain**: Interfaz del caso de uso en `Domain/Application/{Feature}/I{Nombre}UseCase.cs`.
- [ ] **Domain**: Entidad con sufijo `Entity` en `Domain/Entities/` (si aplica).
- [ ] **Domain**: Interfaces de `Repository` (`Domain/Repository/`) y/o `QueryService` (`Domain/QueryService/`).
- [ ] **Application**: Implementación en `Application/UseCases/{Feature}/{Nombre}UseCase.cs`.
- [ ] **Application**: Registrar el caso de uso (`Scoped`) en `Application/ServiceExtension.cs`.
- [ ] **Infrastructure.MySql.QueryService**: Implementar (lectura) con `AppDbContext` + `AsNoTracking()` y `.ToDomain()`; registrar en `AddQueryService`.
- [ ] **Infrastructure.MySql.Repository**: Implementar (escritura) recibiendo entidades; registrar en `AddRepositoryService`.
- [ ] **Infrastructure.MySql**: Añadir/actualizar DTO en `Dtos/` y mapeador en `Mappers/` si hace falta.
- [ ] **Infrastructure.MySql**: Si hay cambios de esquema, extraer el SQL y comunicarlo al usuario.
- [ ] **Host.Api**: Endpoints en `Controllers/Map{Entidad}ControllerExtension.cs` con `.RequireAuthorization()`.
- [ ] **Host.Api**: Registrar el grupo en `Configurations/ApiExtension.cs` (`MapApi()`).
- [ ] **Host.Api**: Verificar que `AppDbContext`, JWT y servicios estén registrados en `Program.cs`.
