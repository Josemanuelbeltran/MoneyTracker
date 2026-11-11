using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using MoneyTracker.Domain.Exceptions;
using MoneyTracker.Host.Api.Controllers;

namespace MoneyTracker.Host.Api.Configurations
{
    public static class ApiExtension
    {
        // Método de extensión para mapear controladores y configurar manejo global de errores
        public static WebApplication MapApi(this WebApplication app)
        {
            // Toda la API cuelga de /api para no chocar con las rutas del SPA Angular
            // (p.ej. /budgets es una página del front y /api/budgets el endpoint).
            var api = app.MapGroup("/api");

            // Registra todos los endpoints definidos en la capa de controladores
            api.AddExpenseCategoryController();
            api.AddIncomeCategoryControllers();
            api.AddExpenseController();
            api.AddIncomeControllers();
            api.AddRecurringControllers();
            api.AddAccountTypeControllers();
            api.AddUserControllers();
            api.AddAccountControllers();
            api.AddTransferControllers();
            api.AddPocketControllers();
            api.AddCategoryBudgetControllers();
            api.AddExpenseReimbursementControllers();
            api.AddPersonControllers();
            api.AddPetControllers();

            // Configura el middleware global para manejar excepciones no controladas
            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    // Obtiene el error capturado por el middleware
                    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                    // Crea un logger dinámico usando ILoggerFactory
                    var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("GlobalExceptionHandler");

                    // Registra el error en los logs (con Serilog o consola)
                    logger.LogError(error, "❌ Excepción no controlada: {Mensaje}", error?.Message);

                    // Determina el código HTTP según el tipo de excepción
                    var statusCode = error switch
                    {
                        UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401
                        ForbiddenOperationException => (int)HttpStatusCode.Forbidden, // 403
                        ValidationException => (int)HttpStatusCode.BadRequest, // 400
                        KeyNotFoundException => (int)HttpStatusCode.NotFound, // 404
                        ArgumentException => (int)HttpStatusCode.BadRequest, // 400
                        _ => (int)HttpStatusCode.InternalServerError // 500 por defecto
                    };

                    // Mensaje amigable según el tipo de error
                    var message = error switch
                    {
                        UnauthorizedAccessException => "No autorizado.",
                        ForbiddenOperationException => error.Message,
                        ValidationException => "Datos inválidos.",
                        KeyNotFoundException => "Recurso no encontrado.",
                        ArgumentException => error.Message,
                        _ => "Ha ocurrido un error interno en el servidor."
                    };

                    // Configura la respuesta HTTP
                    context.Response.StatusCode = statusCode;
                    context.Response.ContentType = "application/json";

                    // Devuelve una respuesta con formato uniforme usando tu clase HTTPResponse<T>
                    var response = Response.HttpResponse<object>.Fail(message, statusCode);
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                });
            });

            return app;
        }
    }
}