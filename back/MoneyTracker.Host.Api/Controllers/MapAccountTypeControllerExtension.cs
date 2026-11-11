using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Domain.Application.AccountType;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.Dtos;    

namespace MoneyTracker.Host.Api.Controllers;

public static class MapAccountTypeControllerExtension
{
    public static void AddAccountTypeControllers(this IEndpointRouteBuilder app)
    {
        // === GET /account-types (listar) ===
        app.MapGet("/account-types", async (
            [FromServices] IGetAccountTypesUseCase useCase) =>
        {
            var types = await useCase.ExecuteAsync();
            return Results.Ok(types);
        }).RequireAuthorization();

        // === POST /account-types ===
        app.MapPost("/account-types", async (
            [FromServices] ICreateAccountTypeUseCase useCase,
            [FromBody] AccountTypeDto body) =>
        {
            if (string.IsNullOrWhiteSpace(body.Name))
                return Results.BadRequest(new { error = "Name es obligatorio" });

            var accountType = await useCase.ExecuteAsync(body.Name);
            return Results.Created($"/account-types/{accountType.Id}", new
            {
                accountType.Id,
                accountType.Name
            });
        }).RequireAuthorization();
        
        // DELETE /account-types/{id}
        app.MapDelete("/account-types/{id:int}", async (
            [FromServices] IAccountTypeRepository repo,
            [FromRoute] int id) =>
        {
            var deleted = await repo.DeleteAsync(id);
            return deleted;

        }).RequireAuthorization();
        
        // === PUT /account-types/{id} ===
        app.MapPut("/account-types/{id:int}", async (
            [FromServices] IUpdateAccountTypeUseCase useCase,
            [FromRoute] int id,
            [FromBody] AccountTypeDto body) =>
        {
            var updated = await useCase.ExecuteAsync(id, body.Name);
            return updated;
        }).RequireAuthorization();
    
    }
}