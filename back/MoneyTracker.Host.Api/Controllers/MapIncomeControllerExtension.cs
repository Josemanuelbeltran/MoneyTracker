using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Application.Income;
using MoneyTracker.Host.Api.Security;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Host.Api.Controllers;

public static class MapIncomeControllerExtension
{
    public static void AddIncomeControllers(this IEndpointRouteBuilder app)
    {
        // Crear ingreso
        app.MapPost("/incomes", async (
            [FromServices] ICreateIncomeUseCase useCase,
            AppDbContext db,
            [FromBody] IncomeDto body,
            HttpContext ctx) =>
        {
            var userId = ctx.GetUserId();

            if (body.AccountId <= 0 || body.CategoryId <= 0 || body.Amount <= 0)
                throw new ValidationException("Datos inválidos");

            if (await ValidateAccountAndCategory(db, userId, body.AccountId, body.CategoryId) is { } error)
                return error;

            // El endpoint construye la entidad de dominio a partir del body (CLAUDE.md)
            var income = body.ToDomain();
            income.Id = 0;                 // alta: id lo asigna la BD
            income.UserId = userId;        // el usuario viene del JWT, no del body
            income.IncomeDate = body.IncomeDate == default
                ? DateOnly.FromDateTime(DateTime.Now)
                : body.IncomeDate;

            var created = await useCase.ExecuteAsync(income);
            return Results.Created($"/incomes/{created.Id}", created);
        }).RequireAuthorization();

        // Crear ingreso recurrente: materializa N meses por adelantado
        app.MapPost("/incomes/recurring", async (
            [FromServices] ICreateRecurringIncomeUseCase useCase,
            AppDbContext db,
            [FromBody] IncomeDto body,
            HttpContext ctx,
            [FromQuery] int monthsAhead = 12) =>
        {
            var userId = ctx.GetUserId();

            if (body.AccountId <= 0 || body.CategoryId <= 0 || body.Amount <= 0)
                throw new ValidationException("Datos inválidos");

            if (await ValidateAccountAndCategory(db, userId, body.AccountId, body.CategoryId) is { } error)
                return error;

            var template = body.ToDomain();
            template.Id = 0;
            template.UserId = userId;
            template.IsRecurring = true;
            template.IncomeDate = body.IncomeDate == default
                ? DateOnly.FromDateTime(DateTime.Now)
                : body.IncomeDate;

            var created = await useCase.ExecuteAsync(template, monthsAhead);
            return Results.Ok(new { created });
        }).RequireAuthorization();

        // Listar ingresos del usuario autenticado
        app.MapGet("/incomes", async (
            [FromServices] IGetIncomesUseCase useCase,
            HttpContext ctx) =>
        {
            var incomes = await useCase.ExecuteAsync(ctx.GetUserId());
            return Results.Ok(incomes);
        }).RequireAuthorization();

        // Obtener ingreso por id
        app.MapGet("/incomes/{id}", async (
            [FromServices] IGetIncomeByIdUseCase useCase,
            HttpContext ctx,
            int id) =>
        {
            var income = await useCase.ExecuteAsync(id, ctx.GetUserId());
            if (income is null)
                return Results.NotFound(new { error = "Income no existe" });
            return Results.Ok(income);
        }).RequireAuthorization();

        // Actualizar ingreso
        app.MapPut("/incomes/{id}", async (
            [FromServices] IUpdateIncomeUseCase useCase,
            AppDbContext db,
            [FromBody] IncomeDto body,
            HttpContext ctx,
            int id) =>
        {
            var userId = ctx.GetUserId();

            if (body.AccountId <= 0 || body.CategoryId <= 0 || body.Amount <= 0)
                throw new ValidationException("Datos inválidos");

            if (await ValidateAccountAndCategory(db, userId, body.AccountId, body.CategoryId) is { } error)
                return error;

            var income = body.ToDomain();
            income.Id = id;                // el id viene de la ruta
            income.UserId = userId;
            income.IncomeDate = body.IncomeDate == default
                ? DateOnly.FromDateTime(DateTime.Now)
                : body.IncomeDate;

            var updated = await useCase.ExecuteAsync(income);
            return Results.Ok(updated);
        }).RequireAuthorization();

        // Borrar ingreso
        app.MapDelete("/incomes/{id}", async (
            [FromServices] IDeleteIncomeUseCase useCase,
            HttpContext ctx,
            int id) =>
        {
            var deleted = await useCase.ExecuteAsync(id, ctx.GetUserId());
            if (!deleted)
                return Results.NotFound(new { error = "Income no existe" });
            return Results.NoContent();
        }).RequireAuthorization();

        // "Editar este y siguientes": cambia el importe de la serie desde este ingreso
        app.MapPut("/incomes/{id}/series", async (
            [FromServices] IUpdateIncomeSeriesUseCase useCase,
            [FromBody] IncomeDto body,
            HttpContext ctx,
            int id) =>
        {
            if (body.Amount <= 0)
                throw new ValidationException("Datos inválidos");

            var updated = await useCase.ExecuteAsync(id, ctx.GetUserId(), body.Amount);
            return Results.Ok(new { updated });
        }).RequireAuthorization();

        // "Borrar este y siguientes": elimina la serie desde este ingreso
        app.MapDelete("/incomes/{id}/series", async (
            [FromServices] IDeleteIncomeSeriesUseCase useCase,
            HttpContext ctx,
            int id) =>
        {
            var deleted = await useCase.ExecuteAsync(id, ctx.GetUserId());
            return Results.Ok(new { deleted });
        }).RequireAuthorization();
    }

    /// <summary>
    /// La cuenta debe ser del usuario y la categoría debe ser una que vea
    /// (común o suya). Devuelve el error a responder, o null si todo va bien.
    /// </summary>
    private static async Task<IResult?> ValidateAccountAndCategory(AppDbContext db, int userId, int accountId, int categoryId)
    {
        var accountOk = await db.Accounts.AnyAsync(a => a.Id == accountId && a.UserId == userId);
        if (!accountOk)
            return Results.NotFound(new { error = "La cuenta no existe o no es tuya" });

        var categoryOk = await db.IncomeCategories
            .AnyAsync(c => c.Id == categoryId && (c.UserId == null || c.UserId == userId));
        if (!categoryOk)
            return Results.NotFound(new { error = "La categoría no existe o no es tuya" });

        return null;
    }
}
