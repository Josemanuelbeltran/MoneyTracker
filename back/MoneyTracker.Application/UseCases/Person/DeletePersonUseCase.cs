using MoneyTracker.Domain.Application.Person;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Person;

public class DeletePersonUseCase : IDeletePersonUseCase
{
    private readonly IPersonRepository _repository;

    public DeletePersonUseCase(IPersonRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> ExecuteAsync(int id, int userId)
    {
        return _repository.DeleteAsync(id, userId);
    }
}
