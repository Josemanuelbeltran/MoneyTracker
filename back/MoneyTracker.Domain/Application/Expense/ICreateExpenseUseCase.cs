using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Expense;

public interface ICreateExpenseUseCase
{
    Task<ExpenseEntity> ExecuteAsync(int userId, int accountId, int categoryId, decimal amount, string description, DateTime expenseDate, int? pocketId = null, int? petId = null);
}
