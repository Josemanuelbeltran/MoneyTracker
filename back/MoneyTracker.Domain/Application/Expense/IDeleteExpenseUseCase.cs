namespace MoneyTracker.Domain.Application.Expense;

public interface IDeleteExpenseUseCase
{
    Task<bool> ExecuteAsync(int id, int userId);
}
