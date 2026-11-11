using MoneyTracker.Domain.Application.Person;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Person;

public class UpdatePersonUseCase : IUpdatePersonUseCase
{
    private readonly IPersonRepository _repository;

    public UpdatePersonUseCase(IPersonRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> ExecuteAsync(int id, int userId, string name)
    {
        var clean = (name ?? string.Empty).Trim();
        if (clean.Length == 0)
            throw new ArgumentException("El nombre no puede estar vacío.");

        return _repository.UpdateAsync(id, userId, clean);
    }
}
