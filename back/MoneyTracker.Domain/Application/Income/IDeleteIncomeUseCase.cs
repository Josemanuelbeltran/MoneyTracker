namespace MoneyTracker.Domain.Application.Income;

public interface IDeleteIncomeUseCase
{
    Task<bool> ExecuteAsync(int id, int userId);
}
