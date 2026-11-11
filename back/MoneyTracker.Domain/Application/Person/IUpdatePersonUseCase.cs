namespace MoneyTracker.Domain.Application.Person;

public interface IUpdatePersonUseCase
{
    Task<bool> ExecuteAsync(int id, int userId, string name);
}
