using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Application.Account;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Host.Api.Security;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Host.Api.Controllers;

public static class MapAccountControllerExtension
{
    public static void AddAccountControllers(this IEndpointRouteBuilder app)
    {

        app.MapGet("/accounts/{id:int}", async (
                HttpContext ctx,
                [FromServices] IGetAccountByIdUseCase useCase,
                [FromRoute] int id) =>
            {
                var account = await useCase.ExecuteAsync(id, ctx.GetUserId());
                if (account is null)
                    return Results.NotFound(new { error = "Account no existe" });

                return Results.Ok(account);
            }
        ).RequireAuthorization();

        // Listar las cuentas del usuario autenticado
        app.MapGet("/accounts", async (
                HttpContext ctx,
                [FromServices] IGetAccountsUseCase useCase) =>
            {
                var accounts = await useCase.ExecuteAsync(ctx.GetUserId());
                return Results.Ok(accounts);
            }
        ).RequireAuthorization();


        app.MapPost("/accounts", async (
                HttpContext ctx,
                AppDbContext db, // inyecta el DbContext aquí para validar
                ICreateAccountUseCase useCase,
                AccountDto body) =>
            {
                var userId = ctx.GetUserId();

                // valida FK explícitamente (da mejores mensajes que esperar al error de BD)
                var userExists = await db.Users.AnyAsync(u => u.Id == userId);
                if (!userExists) return Results.NotFound(new { error = "Usuario no existe" });

                var typeExists = await db.AccountTypes.AnyAsync(t => t.Id == body.AccountTypeId);
                if (!typeExists) return Results.NotFound(new { error = "AccountType no existe" });

                var entity = new AccountEntity
                {
                    UserId = userId,
                    AccountTypeId = body.AccountTypeId,
                    Name = body.Name,
                    Balance = body.Balance
                };

                var created = await useCase.ExecuteAsync(entity);
                return Results.Created($"/accounts/{created!.Id}", created);
            })
            .RequireAuthorization();

        // === DELETE /accounts/{id} ===
        app.MapDelete("/accounts/{id:int}", async (
            HttpContext ctx,
            [FromServices] IDeleteAccountUseCase useCase,
            [FromRoute] int id) =>
        {
            var deleted = await useCase.ExecuteAsync(id, ctx.GetUserId());
            return deleted is true
                ? Results.NoContent()
                : Results.NotFound(new { error = "Account no existe" });
        }).RequireAuthorization();

        // === PATCH /accounts/{id} ===
        app.MapPatch("/accounts/{id:int}", async (
                HttpContext ctx,
                IUpdateAccountUseCase useCase,
                int id,
                AccountPathDto body) =>
            {
                var updated = await useCase.ExecuteAsync(
                    name: body.Name,
                    accountTypeId: body.AccountTypeId,
                    balance: body.Balance,
                    id: id,
                    userId: ctx.GetUserId());

                return updated is null
                    ? Results.NotFound(new { error = "Account no existe" })
                    : Results.Ok(updated);
            })
            .RequireAuthorization();

    }


}
