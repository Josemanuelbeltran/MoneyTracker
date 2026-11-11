using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Domain.Application.Pet;
using MoneyTracker.Host.Api.Security;

namespace MoneyTracker.Host.Api.Controllers;

/// <summary>
/// Tabla auxiliar de mascotas, para el desplegable del gasto. Son de quien las da
/// de alta: cada usuario ve las suyas. El id NO lo genera la BD, es el que tiene
/// la mascota en PetRecording, así que se escribe a mano.
/// </summary>
public static class MapPetControllerExtension
{
    /// <summary>El id NO es opcional: es el que tiene la mascota en PetRecording.</summary>
    public record PetBody(int Id, string Name);

    public static void AddPetControllers(this IEndpointRouteBuilder app)
    {
        // Alta con el id de PetRecording escrito a mano: es el que viaja en el gasto.
        app.MapPost("/pets", async (
                [FromServices] ICreatePetUseCase useCase,
                [FromBody] PetBody body,
                HttpContext ctx) =>
            {
                var pet = await useCase.ExecuteAsync(body.Id, ctx.GetUserId(), body.Name);
                return Results.Created("/pets/" + pet.Id, pet);
            })
            .RequireAuthorization();

        app.MapGet("/pets", async (
                [FromServices] IGetPetsUseCase useCase,
                HttpContext ctx) =>
            {
                var pets = await useCase.ExecuteAsync(ctx.GetUserId());
                return Results.Ok(pets);
            })
            .RequireAuthorization();
    }
}
