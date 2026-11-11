using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.CategoryBudget;

public interface IGetBudgetsUseCase
{
    Task<List<CategoryBudgetEntity>> ExecuteAsync(int userId);
}
