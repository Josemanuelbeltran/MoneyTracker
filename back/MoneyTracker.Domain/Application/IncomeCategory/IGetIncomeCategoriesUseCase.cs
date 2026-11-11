using MoneyTracker.Domain.Entities.Categories;

namespace MoneyTracker.Domain.Application.IncomeCategory;

public interface IGetIncomeCategoriesUseCase
{
    Task<List<IncomeCategoryEntity>> ExecuteAsync(int userId);
}
