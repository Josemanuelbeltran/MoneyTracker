using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Domain.Application.User;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Host.Api.Controllers;

public static class MapUserControllerExtension
{

    public static void AddUserControllers(this IEndpointRouteBuilder app)
    {

        // === POST /auth/login ===
        app.MapPost("/auth/login", async (
            [FromServices] ILoginUseCase loginUseCase,
            [FromBody] UserDto dto) =>
        {

            var token = await loginUseCase.ExecuteAsync(dto.Name, dto.PasswordHash);
            if (token is null)
                return Results.Unauthorized(); // 401

            return Results.Ok(new
            {
                token
            });
        });

        // === POST /users ===
        app.MapPost("/users", async (
            [FromServices] IRegisterUseCase registerUseCase,
            [FromBody] UserDto dto) =>
        {
            var user = await registerUseCase.ExecuteAsync(
                dto.Name,
                dto.Email,
                dto.PasswordHash,
                dto.PhoneNumber,
                dto.VerifiedPhone
            );

            // respuesta sin exponer PasswordHash
            return Results.Created($"/users/{user.Id}", new
            {
                user.Name,
                user.Email,
                user.PhoneNumber,
                user.VerifiedPhone
            });
        });
        }
}