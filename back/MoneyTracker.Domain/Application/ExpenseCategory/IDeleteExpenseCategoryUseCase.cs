namespace MoneyTracker.Domain.Application.ExpenseCategory;

public interface IDeleteExpenseCategoryUseCase
{
    Task<bool> ExecuteAsync(int id, int userId, bool isAdmin);
}
