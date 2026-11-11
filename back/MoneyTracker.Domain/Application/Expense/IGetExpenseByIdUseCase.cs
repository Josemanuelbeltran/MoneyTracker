using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Expense;

public interface IGetExpenseByIdUseCase
{
    Task<ExpenseEntity?> ExecuteAsync(int id, int userId);
}
