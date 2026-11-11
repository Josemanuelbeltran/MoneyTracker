using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Expense;

public interface IUpdateExpenseUseCase
{
    Task<ExpenseEntity> ExecuteAsync(ExpenseEntity expense);
}
