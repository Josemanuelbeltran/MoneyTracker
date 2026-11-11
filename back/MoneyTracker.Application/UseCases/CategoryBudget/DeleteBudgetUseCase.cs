using MoneyTracker.Domain.Application.CategoryBudget;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.CategoryBudget;

public class DeleteBudgetUseCase : IDeleteBudgetUseCase
{
    private readonly ICategoryBudgetRepository _repository;

    public DeleteBudgetUseCase(ICategoryBudgetRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> ExecuteAsync(int id, int userId)
    {
        return _repository.DeleteAsync(id, userId);
    }
}
