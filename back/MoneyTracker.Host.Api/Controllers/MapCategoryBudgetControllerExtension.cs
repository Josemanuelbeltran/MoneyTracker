using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Application.CategoryBudget;
using MoneyTracker.Host.Api.Security;
using MoneyTracker.Infrastructure.MySql.DDBBConection;

namespace MoneyTracker.Host.Api.Controllers;

public record BudgetUpsertDto(int CategoryId, decimal MonthlyLimit);

public static class MapCategoryBudgetControllerExtension
{
    public static void AddCategoryBudgetControllers(this IEndpointRouteBuilder app)
    {
        // Listar los presupuestos del usuario autenticado
        app.MapGet("/budgets", async (
                HttpContext ctx,
                [FromServices] IGetBudgetsUseCase useCase) =>
            {
                var budgets = await useCase.ExecuteAsync(ctx.GetUserId());
                return Results.Ok(budgets);
            })
            .RequireAuthorization();

        // Crear o actualizar el presupuesto de una categoría
        app.MapPost("/budgets", async (
                HttpContext ctx,
                AppDbContext db,
                [FromServices] IUpsertBudgetUseCase useCase,
                BudgetUpsertDto body) =>
            {
                var userId = ctx.GetUserId();

                if (body.MonthlyLimit < 0)
                    return Results.BadRequest(new { error = "El presupuesto no puede ser negativo" });

                // Solo se presupuestan categorías que el usuario ve: comunes o suyas.
                var categoryOk = await db.ExpenseCategories
                    .AnyAsync(c => c.Id == body.CategoryId && (c.UserId == null || c.UserId == userId));
                if (!categoryOk)
                    return Results.NotFound(new { error = "La categoría de gasto no existe" });

                var budget = await useCase.ExecuteAsync(userId, body.CategoryId, body.MonthlyLimit);
                return Results.Ok(budget);
            })
            .RequireAuthorization();

        // Eliminar un presupuesto
        app.MapDelete("/budgets/{id:int}", async (
                HttpContext ctx,
                [FromServices] IDeleteBudgetUseCase useCase,
                [FromRoute] int id) =>
            {
                var deleted = await useCase.ExecuteAsync(id, ctx.GetUserId());
                return deleted
                    ? Results.NoContent()
                    : Results.NotFound(new { error = "Presupuesto no existe" });
            })
            .RequireAuthorization();
    }
}
