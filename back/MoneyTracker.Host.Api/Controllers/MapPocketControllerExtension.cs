using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Application.Pocket;
using MoneyTracker.Host.Api.Security;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Host.Api.Controllers;

public record PocketMoveDto(int? FromPocketId, int? ToPocketId, decimal Amount);

public record PocketNameDto(string Name);

public static class MapPocketControllerExtension
{
    public static void AddPocketControllers(this IEndpointRouteBuilder app)
    {
        // Listar los pockets del usuario autenticado
        app.MapGet("/pockets", async (
                HttpContext ctx,
                [FromServices] IGetPocketsUseCase useCase) =>
            {
                var pockets = await useCase.ExecuteAsync(ctx.GetUserId());
                return Results.Ok(pockets);
            })
            .RequireAuthorization();

        // Crear pocket dentro de una cuenta del usuario
        app.MapPost("/pockets", async (
                HttpContext ctx,
                AppDbContext db,
                [FromServices] ICreatePocketUseCase useCase,
                PocketDto body) =>
            {
                var userId = ctx.GetUserId();

                if (string.IsNullOrWhiteSpace(body.Name))
                    return Results.BadRequest(new { error = "El pocket necesita un nombre" });

                var accountOk = await db.Accounts.AnyAsync(a => a.Id == body.AccountId && a.UserId == userId);
                if (!accountOk)
                    return Results.NotFound(new { error = "La cuenta no existe o no es tuya" });

                var created = await useCase.ExecuteAsync(userId, body.AccountId, body.Name, body.Balance);
                return Results.Created($"/pockets/{created.Id}", created);
            })
            .RequireAuthorization();

        // Renombrar pocket
        app.MapPatch("/pockets/{id:int}", async (
                HttpContext ctx,
                [FromServices] IUpdatePocketUseCase useCase,
                [FromRoute] int id,
                PocketNameDto body) =>
            {
                var updated = await useCase.ExecuteAsync(id, body.Name, ctx.GetUserId());
                return updated is null
                    ? Results.NotFound(new { error = "Pocket no existe" })
                    : Results.Ok(updated);
            })
            .RequireAuthorization();

        // Eliminar pocket
        app.MapDelete("/pockets/{id:int}", async (
                HttpContext ctx,
                [FromServices] IDeletePocketUseCase useCase,
                [FromRoute] int id) =>
            {
                var deleted = await useCase.ExecuteAsync(id, ctx.GetUserId());
                return deleted
                    ? Results.NoContent()
                    : Results.NotFound(new { error = "Pocket no existe" });
            })
            .RequireAuthorization();

        // Mover dinero entre pockets (o entre "sin asignar" y un pocket)
        app.MapPost("/pockets/move", async (
                HttpContext ctx,
                [FromServices] IMovePocketUseCase useCase,
                PocketMoveDto body) =>
            {
                await useCase.ExecuteAsync(body.FromPocketId, body.ToPocketId, body.Amount, ctx.GetUserId());
                return Results.NoContent();
            })
            .RequireAuthorization();
    }
}
