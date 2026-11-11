using MoneyTracker.Domain.Application.Pet;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Pet;

public class CreatePetUseCase : ICreatePetUseCase
{
    private readonly IPetRepository _repository;
    private readonly IPetQueryService _queryService;

    public CreatePetUseCase(IPetRepository repository, IPetQueryService queryService)
    {
        _repository = repository;
        _queryService = queryService;
    }

    public async Task<PetEntity> ExecuteAsync(int id, int userId, string name)
    {
        var clean = (name ?? string.Empty).Trim();
        if (id <= 0)
            throw new ArgumentException("El id de la mascota es el que tiene en PetRecording, y tiene que ser mayor que 0.");
        if (clean.Length == 0)
            throw new ArgumentException("El nombre no puede estar vacío.");
        if (clean.Length > 100)
            throw new ArgumentException("El nombre no puede pasar de 100 caracteres.");

        // Solo se mira entre las TUYAS: que otro tenga ese id no es asunto tuyo.
        var existing = await _queryService.GetByIdAsync(id, userId);
        if (existing is not null)
        {
            // Darla de alta dos veces no deberia petar: te devolvemos la que hay.
            if (string.Equals(existing.Name, clean, StringComparison.OrdinalIgnoreCase))
                return existing;

            // Pero si ese id ya es de OTRA mascota tuya, avisamos: el id manda, y
            // apuntar los gastos a la mascota equivocada no se arregla luego facilmente.
            throw new ArgumentException($"El id {id} ya es de «{existing.Name}». Mira en PetRecording cuál es el id correcto.");
        }

        return await _repository.CreateAsync(new PetEntity(id, userId, clean));
    }
}
