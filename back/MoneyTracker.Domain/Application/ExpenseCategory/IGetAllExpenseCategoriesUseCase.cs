using MoneyTracker.Domain.Entities.Categories;

namespace MoneyTracker.Domain.Application.ExpenseCategory;

public interface IGetAllExpenseCategoriesUseCase
{
    Task<List<ExpenseCategoryEntity>> ExecuteAsync(int userId);
}
