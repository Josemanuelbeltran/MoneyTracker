using MoneyTracker.Domain.Application.IncomeCategory;
using MoneyTracker.Domain.Entities.Categories;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.IncomeCategories;

public class GetIncomeCategoriesUseCase : IGetIncomeCategoriesUseCase
{
    private readonly IIncomeCategoryQueryService _incomeCategoryQueryService;

    public GetIncomeCategoriesUseCase(IIncomeCategoryQueryService incomeCategoryQueryService)
    {
        _incomeCategoryQueryService = incomeCategoryQueryService;
    }

    public Task<List<IncomeCategoryEntity>> ExecuteAsync(int userId)
    {
        return _incomeCategoryQueryService.GetAllAsync(userId);
    }
}
