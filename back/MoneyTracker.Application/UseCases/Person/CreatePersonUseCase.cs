using MoneyTracker.Domain.Application.Person;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Person;

public class CreatePersonUseCase : ICreatePersonUseCase
{
    private readonly IPersonRepository _repository;

    public CreatePersonUseCase(IPersonRepository repository)
    {
        _repository = repository;
    }

    public async Task<PersonEntity> ExecuteAsync(int userId, string name)
    {
        var clean = (name ?? string.Empty).Trim();
        if (clean.Length == 0)
            throw new ArgumentException("El nombre no puede estar vacío.");
        if (clean.Length > 100)
            throw new ArgumentException("El nombre no puede pasar de 100 caracteres.");

        // Añadir a alguien que ya tienes no debería petar: te devolvemos el que hay.
        var existing = await _repository.GetByNameAsync(userId, clean);
        if (existing is not null)
            return existing;

        return await _repository.CreateAsync(new PersonEntity(userId, clean));
    }
}
