using MoneyTracker.Domain.Application.CategoryBudget;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.CategoryBudget;

public class GetBudgetsUseCase : IGetBudgetsUseCase
{
    private readonly ICategoryBudgetQueryService _queryService;

    public GetBudgetsUseCase(ICategoryBudgetQueryService queryService)
    {
        _queryService = queryService;
    }

    public Task<List<CategoryBudgetEntity>> ExecuteAsync(int userId)
    {
        return _queryService.GetByUserAsync(userId);
    }
}
