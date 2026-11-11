using MoneyTracker.Domain.Application.ExpenseCategory;
using MoneyTracker.Domain.Exceptions;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace ClassLibrary1MoneyTracker.Application.UseCases.ExpenseCategories;

public class DeleteExpenseCategoryUseCase : IDeleteExpenseCategoryUseCase
{
    private readonly IExpenseCategoryRepository _repository;
    private readonly IExpenseCategoryQueryService _query;

    public DeleteExpenseCategoryUseCase(IExpenseCategoryRepository repository, IExpenseCategoryQueryService query)
    {
        _repository = repository;
        _query = query;
    }

    public async Task<bool> ExecuteAsync(int id, int userId, bool isAdmin)
    {
        // Si no la ve, para él no existe (no se filtra si es de otro usuario).
        var current = await _query.GetByIdAsync(id, userId);
        if (current is null) return false;

        if (current.IsCommon && !isAdmin)
            throw new ForbiddenOperationException("Las categorías comunes solo las puede borrar un administrador.");

        return await _repository.DeleteAsync(id);
    }
}
