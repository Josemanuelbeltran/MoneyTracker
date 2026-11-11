using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Domain.Application.Person;
using MoneyTracker.Host.Api.Security;

namespace MoneyTracker.Host.Api.Controllers;

/// <summary>
/// Agenda de personas con las que compartes gastos: las que te hacen el bizum.
/// Cada usuario tiene la suya.
/// </summary>
public static class MapPersonControllerExtension
{
    public record PersonBody(string Name);

    public static void AddPersonControllers(this IEndpointRouteBuilder app)
    {
        app.MapGet("/people", async (
                [FromServices] IGetPeopleUseCase useCase,
                HttpContext ctx) =>
            {
                var people = await useCase.ExecuteAsync(ctx.GetUserId());
                return Results.Ok(people);
            })
            .RequireAuthorization();

        // Crear desde el propio formulario del gasto, sin salir del modal.
        // Si ya tienes a alguien con ese nombre, devuelve el que hay (no duplica).
        app.MapPost("/people", async (
                [FromServices] ICreatePersonUseCase useCase,
                [FromBody] PersonBody body,
                HttpContext ctx) =>
            {
                var person = await useCase.ExecuteAsync(ctx.GetUserId(), body.Name);
                return Results.Created($"/people/{person.Id}", person);
            })
            .RequireAuthorization();

        app.MapPatch("/people/{id:int}", async (
                [FromServices] IUpdatePersonUseCase useCase,
                int id,
                [FromBody] PersonBody body,
                HttpContext ctx) =>
            {
                var updated = await useCase.ExecuteAsync(id, ctx.GetUserId(), body.Name);
                return updated ? Results.NoContent() : Results.NotFound();
            })
            .RequireAuthorization();

        // Al borrar, los reembolsos que la apuntaban se quedan con el nombre guardado.
        app.MapDelete("/people/{id:int}", async (
                [FromServices] IDeletePersonUseCase useCase,
                int id,
                HttpContext ctx) =>
            {
                var deleted = await useCase.ExecuteAsync(id, ctx.GetUserId());
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .RequireAuthorization();
    }
}
