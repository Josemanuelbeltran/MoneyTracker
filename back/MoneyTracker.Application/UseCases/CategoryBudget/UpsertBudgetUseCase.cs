using MoneyTracker.Domain.Application.CategoryBudget;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.CategoryBudget;

public class UpsertBudgetUseCase : IUpsertBudgetUseCase
{
    private readonly ICategoryBudgetRepository _repository;

    public UpsertBudgetUseCase(ICategoryBudgetRepository repository)
    {
        _repository = repository;
    }

    public Task<CategoryBudgetEntity> ExecuteAsync(int userId, int categoryId, decimal monthlyLimit)
    {
        if (monthlyLimit < 0)
            throw new ArgumentException("El presupuesto no puede ser negativo.");

        return _repository.UpsertAsync(userId, categoryId, monthlyLimit);
    }
}
