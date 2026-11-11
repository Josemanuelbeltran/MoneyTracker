using MoneyTracker.Domain.Application.ExpenseCategory;
using MoneyTracker.Domain.Entities.Categories;
using MoneyTracker.Domain.QueryService;

namespace ClassLibrary1MoneyTracker.Application.UseCases.ExpenseCategories;

public class GetExpenseCategoryByIdUseCase : IGetExpenseCategoryByIdUseCase
{
    private readonly IExpenseCategoryQueryService _expenseCategoryQueryService;

    public GetExpenseCategoryByIdUseCase(IExpenseCategoryQueryService expenseCategoryQueryService)
    {
        _expenseCategoryQueryService = expenseCategoryQueryService;
    }

    public async Task<ExpenseCategoryEntity?> ExecuteAsync(int id, int userId)
    {
        var dto = await _expenseCategoryQueryService.GetByIdWithChildrenAsync(id, userId);
        return dto;
    }
}
