using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Expense;

public interface IGetExpensesUseCase
{
    Task<List<ExpenseEntity>> ExecuteAsync(int userId);
}
