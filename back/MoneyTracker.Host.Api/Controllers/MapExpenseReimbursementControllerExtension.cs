using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Application.ExpenseReimbursement;
using MoneyTracker.Host.Api.Security;
using MoneyTracker.Infrastructure.MySql.DDBBConection;

namespace MoneyTracker.Host.Api.Controllers;

/// <summary>
/// Reembolsos de gastos compartidos: lo que adelantaste de otra gente y te tienen
/// que devolver (el bizum). Mientras está pendiente el dinero sigue fuera de tu
/// cuenta; al marcarlo cobrado entra en la cuenta de destino.
/// </summary>
public static class MapExpenseReimbursementControllerExtension
{
    /// <summary>Lo que se puede cambiar de un reembolso. Lo que llegue null se deja como está.</summary>
    public record ReimbursementPatchBody(decimal? Amount, int? ToAccountId, int? PersonId, string? Debtor);

    public static void AddExpenseReimbursementControllers(this IEndpointRouteBuilder app)
    {
        // Todos los reembolsos del usuario, primero los pendientes.
        app.MapGet("/reimbursements", async (
                [FromServices] IGetReimbursementsUseCase useCase,
                HttpContext ctx) =>
            {
                var reimbursements = await useCase.ExecuteAsync(ctx.GetUserId());
                return Results.Ok(reimbursements);
            })
            .RequireAuthorization();

        // Cambiar lo pactado: te dijeron bizum y al final te lo dan en efectivo, te
        // devuelven otra cantidad, o te lo debe otra persona.
        app.MapPatch("/reimbursements/{id:int}", async (
                [FromServices] IUpdateReimbursementUseCase useCase,
                AppDbContext db,
                int id,
                [FromBody] ReimbursementPatchBody body,
                HttpContext ctx) =>
            {
                var userId = ctx.GetUserId();

                if (body.ToAccountId is int toAccountId)
                {
                    var accountOk = await db.Accounts.AnyAsync(a => a.Id == toAccountId && a.UserId == userId);
                    if (!accountOk)
                        return Results.NotFound(new { error = "La cuenta no existe o no es tuya" });
                }

                // personId = 0 es "quitar la persona", así que solo validamos los ids reales.
                if (body.PersonId is int personId && personId > 0)
                {
                    var personOk = await db.People.AnyAsync(x => x.Id == personId && x.UserId == userId);
                    if (!personOk)
                        return Results.NotFound(new { error = "La persona no existe o no es tuya" });
                }

                var updated = await useCase.ExecuteAsync(
                    id, userId, body.Amount, body.ToAccountId, body.PersonId, body.Debtor);

                return updated ? Results.NoContent() : Results.NotFound();
            })
            .RequireAuthorization();

        // Ya te han hecho el bizum: el dinero entra en la cuenta de destino.
        app.MapPost("/reimbursements/{id:int}/settle", async (
                [FromServices] ISettleReimbursementUseCase useCase,
                int id,
                HttpContext ctx) =>
            {
                var settled = await useCase.ExecuteAsync(id, ctx.GetUserId(), settled: true);
                return settled ? Results.NoContent() : Results.NotFound();
            })
            .RequireAuthorization();

        // Deshacer: vuelve a quedar pendiente de cobro.
        app.MapPost("/reimbursements/{id:int}/unsettle", async (
                [FromServices] ISettleReimbursementUseCase useCase,
                int id,
                HttpContext ctx) =>
            {
                var unsettled = await useCase.ExecuteAsync(id, ctx.GetUserId(), settled: false);
                return unsettled ? Results.NoContent() : Results.NotFound();
            })
            .RequireAuthorization();

        // Borrar el reembolso sin borrar el gasto (por ejemplo, al final lo pagaste tú entero).
        app.MapDelete("/reimbursements/{id:int}", async (
                [FromServices] IDeleteReimbursementUseCase useCase,
                int id,
                HttpContext ctx) =>
            {
                var deleted = await useCase.ExecuteAsync(id, ctx.GetUserId());
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .RequireAuthorization();
    }
}
