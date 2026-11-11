using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Application.Transfer;
using MoneyTracker.Host.Api.Security;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Host.Api.Controllers;

public static class MapTransferControllerExtension
{
    public static void AddTransferControllers(this IEndpointRouteBuilder app)
    {
        // Listar las transferencias del usuario autenticado
        app.MapGet("/transfers", async (
                HttpContext ctx,
                [FromServices] IGetTransfersUseCase useCase) =>
            {
                var transfers = await useCase.ExecuteAsync(ctx.GetUserId());
                return Results.Ok(transfers);
            })
            .RequireAuthorization();

        // Crear transferencia entre dos cuentas del usuario
        app.MapPost("/transfers", async (
                HttpContext ctx,
                AppDbContext db,
                [FromServices] ICreateTransferUseCase useCase,
                TransferDto body) =>
            {
                var userId = ctx.GetUserId();

                if (body.Amount <= 0)
                    return Results.BadRequest(new { error = "El importe debe ser mayor que 0" });
                if (body.FromAccountId == body.ToAccountId)
                    return Results.BadRequest(new { error = "La cuenta de origen y destino no pueden ser la misma" });

                // Ambas cuentas deben existir y pertenecer al usuario autenticado.
                var validAccounts = await db.Accounts
                    .CountAsync(a => a.UserId == userId && (a.Id == body.FromAccountId || a.Id == body.ToAccountId));
                if (validAccounts < 2)
                    return Results.NotFound(new { error = "Alguna de las cuentas no existe o no es tuya" });

                var date = body.TransferDate == default ? DateTime.Now : body.TransferDate;

                var created = await useCase.ExecuteAsync(
                    userId,
                    body.FromAccountId,
                    body.ToAccountId,
                    body.Amount,
                    body.Description,
                    date);

                return Results.Created($"/transfers/{created.Id}", created);
            })
            .RequireAuthorization();

        // Eliminar transferencia
        app.MapDelete("/transfers/{id:int}", async (
                HttpContext ctx,
                [FromServices] IDeleteTransferUseCase useCase,
                [FromRoute] int id) =>
            {
                var deleted = await useCase.ExecuteAsync(id, ctx.GetUserId());
                return deleted
                    ? Results.NoContent()
                    : Results.NotFound(new { error = "Transfer no existe" });
            })
            .RequireAuthorization();
    }
}
