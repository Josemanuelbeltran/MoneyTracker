using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Domain.Application.IncomeCategory;
using MoneyTracker.Domain.Exceptions;
using MoneyTracker.Host.Api.Security;

namespace MoneyTracker.Host.Api.Controllers;

/// <summary>Body de categoría de ingreso. <c>IsCommon</c> solo lo respeta un admin.</summary>
public record IncomeCategoryBodyDto(string Name, int? ParentId, bool? IsCommon);

public static class MapIncomeCategoryControllerExtension
{
    public static void AddIncomeCategoryControllers(this IEndpointRouteBuilder app)
    {
        // Categorías visibles: las comunes + las del usuario
        app.MapGet("/income-categories", async (
            HttpContext ctx,
            [FromServices] IGetIncomeCategoriesUseCase useCase) =>
        {
            var categories = await useCase.ExecuteAsync(ctx.GetUserId());
            return Results.Ok(categories);
        }).RequireAuthorization();

        app.MapPost("/income-categories", async (
            HttpContext ctx,
            [FromServices] ICreateIncomeCategoryUseCase useCase,
            [FromBody] IncomeCategoryBodyDto body) =>
        {
            if (string.IsNullOrWhiteSpace(body.Name))
                return Results.BadRequest(new { error = "Name es obligatorio" });

            try
            {
                var created = await useCase.ExecuteAsync(
                    body.ParentId, body.Name, body.IsCommon ?? false, ctx.GetUserId(), ctx.IsAdmin());

                return Results.Created($"/income-categories/{created.Id}", new
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

        app.MapPatch("/income-categories/{id:int}", async (
            HttpContext ctx,
            [FromServices] IUpdateIncomeCategoryUseCase useCase,
            int id,
            [FromBody] IncomeCategoryBodyDto body) =>
        {
            if (string.IsNullOrWhiteSpace(body.Name))
                return Results.BadRequest(new { error = "Name es obligatorio" });

            try
            {
                var updated = await useCase.ExecuteAsync(id, body.Name, body.IsCommon, ctx.GetUserId(), ctx.IsAdmin());
                return updated is null
                    ? Results.NotFound(new { error = "IncomeCategory no existe" })
                    : Results.Ok(updated);
            }
            catch (ForbiddenOperationException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
            }
        }).RequireAuthorization();

        app.MapDelete("/income-categories/{id:int}", async (
            HttpContext ctx,
            [FromServices] IDeleteIncomeCategoryUseCase useCase,
            int id) =>
        {
            try
            {
                var deleted = await useCase.ExecuteAsync(id, ctx.GetUserId(), ctx.IsAdmin());
                return deleted
                    ? Results.NoContent()
                    : Results.NotFound(new { error = "IncomeCategory no existe" });
            }
            catch (ForbiddenOperationException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
            }
            catch (Exception)
            {
                return Results.Conflict(new { error = "No se puede borrar: la categoría tiene ingresos o subcategorías asociados." });
            }
        }).RequireAuthorization();
    }
}
