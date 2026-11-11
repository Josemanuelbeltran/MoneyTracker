using MoneyTracker.Domain.Entities.Categories;

namespace MoneyTracker.Domain.Application.ExpenseCategory;

public interface IGetExpenseCategoryByIdUseCase
{
    Task<ExpenseCategoryEntity?> ExecuteAsync(int id, int userId);
}
