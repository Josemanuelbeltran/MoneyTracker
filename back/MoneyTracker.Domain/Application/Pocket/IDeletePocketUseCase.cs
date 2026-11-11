namespace MoneyTracker.Domain.Application.Pocket;

public interface IDeletePocketUseCase
{
    Task<bool> ExecuteAsync(int id, int userId);
}
