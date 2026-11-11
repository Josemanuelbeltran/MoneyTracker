using MoneyTracker.Domain.Application.ExpenseCategory;
using MoneyTracker.Domain.Entities.Categories;
using MoneyTracker.Domain.QueryService;

namespace ClassLibrary1MoneyTracker.Application.UseCases.ExpenseCategories;

public class GetAllExpenseCategoriesUseCase : IGetAllExpenseCategoriesUseCase
{
    private IExpenseCategoryQueryService _expenseCategoryQueryService;

    public GetAllExpenseCategoriesUseCase(IExpenseCategoryQueryService expenseCategoryQueryService)
    {
        _expenseCategoryQueryService = expenseCategoryQueryService;
    }

    public async Task<List<ExpenseCategoryEntity>> ExecuteAsync(int userId)
    {
        var dtos = await _expenseCategoryQueryService.GetAllTreeAsync(userId);
        return dtos;
    }
}
