using ClassLibrary1MoneyTracker.Application;
using MoneyTracker.Host.Api.Configurations;
using MoneyTracker.Infrastructure.MySql;
using MoneyTracker.Infrastructure.MySql.Migrations;
using MoneyTracker.Infrastructure.MySql.QueryService;
using MoneyTracker.Infrastructure.MySql.Repository;

var builder = WebApplication.CreateBuilder(args);

const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
{
    // En desarrollo permite cualquier origen localhost (el puerto del front puede variar:
    // 8100/8101 con ionic serve, 4200 con ng serve...).
    options.AddPolicy(DevCorsPolicy, policy =>
        policy.SetIsOriginAllowed(origin =>
                  {
                      if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
                      return uri.Host is "localhost" or "127.0.0.1";
                  })
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddOpenApi();
builder.Services.AddMySqlPersistence(builder.Configuration);
builder.Services.AddApplicationService(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);   // Authentication
builder.Services.AddAuthorization();                            // Authorization
builder.Services.AddQueryService();
builder.Services.AddRepositoryService();

var app = builder.Build();

// Aplica las migraciones de BD (DbUp) al arrancar, antes de servir peticiones.
DatabaseMigrator.Run(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en appsettings.json"));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // app.UseSwaggerUI();
}

// Sirve el frontend Angular compilado (copiado a wwwroot en el despliegue).
// En local wwwroot está vacío (el front corre aparte), así que no molesta.
app.UseDefaultFiles();
app.UseStaticFiles();

// Orden correcto
app.UseCors(DevCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapApi();   // mapea endpoints después de auth/authorization

// Cualquier ruta que no sea de la API devuelve el index.html (rutas del SPA Angular).
app.MapFallbackToFile("index.html");

app.Run();