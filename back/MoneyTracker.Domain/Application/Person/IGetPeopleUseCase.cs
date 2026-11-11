using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Person;

public interface IGetPeopleUseCase
{
    Task<List<PersonEntity>> ExecuteAsync(int userId);
}
