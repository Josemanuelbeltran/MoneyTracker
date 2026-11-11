using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Domain.Application.ExpenseCategory;
using MoneyTracker.Domain.Exceptions;
using MoneyTracker.Host.Api.Security;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Host.Api.Controllers;

/// <summary>Body de alta de categoría de gasto. <c>IsCommon</c> solo lo respeta un admin.</summary>
public record ExpenseCategoryCreateDto(string Name, int? ParentId, bool IsCommon = false);

public static class MapExpenseCategoryControllerExtension
{
    public static void AddExpenseCategoryController(this IEndpointRouteBuilder app)
    {
        app.MapPost("/expense-categories", async (
             HttpContext ctx,
             [FromServices] ICreateExpenseCategoryUseCase useCase,
             [FromBody] ExpenseCategoryCreateDto body) =>
            {
                var userId = ctx.GetUserId();

                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(body.Name))
                    return Results.BadRequest(new { error = "Name es obligatorio" });

                if (body.Name.Length > 100)
                    return Results.BadRequest(new { error = "Name no puede superar 100 caracteres" });

                try
                {
                    // La categoría es del usuario salvo que un admin la marque como común
                    var created = await useCase.ExecuteAsync(body.ParentId, body.Name, body.IsCommon, userId, ctx.IsAdmin());

                    // 201 + Location al recurso recién creado
                    return Results.Created($"/expense-categories/{created.Id}", new
                    {
                        created.Id,
                        created.Name,
                        created.ParentId,
                        created.UserId,
                        created.IsCommon,
                    });
                }
                catch (ForbiddenOperationException ex)
                {
                    return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
                }
        }).RequireAuthorization();

        app.MapGet("/expense-categories/{id:int}", async (
                HttpContext ctx,
                [FromServices] IGetExpenseCategoryByIdUseCase useCase,
                int id
            ) =>
            {
                var result = await useCase.ExecuteAsync(id, ctx.GetUserId());
                return result is null
                    ? Results.NotFound(new { error = "ExpenseCategory no existe" })
                    : Results.Ok(result);
            })
            .RequireAuthorization();

        // Árbol de categorías visibles: las comunes + las del usuario
        app.MapGet("/expense-categories", async (
                HttpContext ctx,
                [FromServices] IGetAllExpenseCategoriesUseCase useCase
            ) =>
            {
                var tree = await useCase.ExecuteAsync(ctx.GetUserId());
                return Results.Ok(tree);
            })
            .RequireAuthorization();

        app.MapDelete("/expense-categories/{id:int}", async (
                HttpContext ctx,
                [FromServices] IDeleteExpenseCategoryUseCase useCase,
                int id
            ) =>
            {
                try
                {
                    var result = await useCase.ExecuteAsync(id, ctx.GetUserId(), ctx.IsAdmin());
                    return result is false
                        ? Results.NotFound(new { error = "ExpenseCategory no existe" })
                        : Results.NoContent();
                }
                catch (ForbiddenOperationException ex)
                {
                    return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
                }
                catch (Exception)
                {
                    return Results.Conflict(new { error = "No se puede borrar: la categoría tiene gastos o subcategorías asociados." });
                }
            })
            .RequireAuthorization();


        app.MapPatch("/expense-categories/{id:int}", async (
                HttpContext ctx,
                [FromServices] IUpdateExpenseCategoryUseCase useCase,
                int id,
                [FromBody] ExpenseCategoryPatchDto body
            ) =>
            {
                try
                {
                    var updated = await useCase.ExecuteAsync(id, body.Name, body.ParentId, body.IsCommon, ctx.GetUserId(), ctx.IsAdmin());
                    return updated is null
                        ? Results.NotFound(new { error = "ExpenseCategory no existe" })
                        : Results.Ok(updated);
                }
                catch (ForbiddenOperationException ex)
                {
                    return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
                }
            })
            .RequireAuthorization();




    }
}
