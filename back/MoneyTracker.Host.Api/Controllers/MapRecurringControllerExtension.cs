using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Domain.Application.Recurring;

namespace MoneyTracker.Host.Api.Controllers;

public static class MapRecurringControllerExtension
{
    public static void AddRecurringControllers(this IEndpointRouteBuilder app)
    {
        // Genera los movimientos fijos que falten en el mes actual.
        app.MapPost("/recurring/generate", async (
            [FromServices] IGenerateRecurringUseCase useCase,
            HttpContext ctx) =>
        {
            var claim = ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)
                        ?? ctx.User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? throw new UnauthorizedAccessException("Token sin identificador de usuario");

            var userId = int.Parse(claim.Value);
            var created = await useCase.ExecuteAsync(userId);
            return Results.Ok(new { created });
        }).RequireAuthorization();
    }
}
