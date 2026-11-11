namespace MoneyTracker.Domain.Application.CategoryBudget;

public interface IDeleteBudgetUseCase
{
    Task<bool> ExecuteAsync(int id, int userId);
}
