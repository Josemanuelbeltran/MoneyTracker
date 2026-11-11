using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.QueryService;

public interface ICategoryBudgetQueryService
{
    Task<List<CategoryBudgetEntity>> GetByUserAsync(int userId);
}
