namespace MoneyTracker.Domain.Application.Person;

public interface IDeletePersonUseCase
{
    Task<bool> ExecuteAsync(int id, int userId);
}
