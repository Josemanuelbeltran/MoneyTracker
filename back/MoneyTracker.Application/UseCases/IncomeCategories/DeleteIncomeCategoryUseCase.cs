using MoneyTracker.Domain.Application.IncomeCategory;
using MoneyTracker.Domain.Exceptions;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.IncomeCategories;

public class DeleteIncomeCategoryUseCase : IDeleteIncomeCategoryUseCase
{
    private readonly IIncomeCategoryRepository _incomeCategoryRepository;
    private readonly IIncomeCategoryQueryService _query;

    public DeleteIncomeCategoryUseCase(
        IIncomeCategoryRepository incomeCategoryRepository,
        IIncomeCategoryQueryService query)
    {
        _incomeCategoryRepository = incomeCategoryRepository;
        _query = query;
    }

    public async Task<bool> ExecuteAsync(int id, int userId, bool isAdmin)
    {
        // Si no la ve, para él no existe (no se filtra si es de otro usuario).
        var current = await _query.GetByIdAsync(id, userId);
        if (current is null) return false;

        if (current.IsCommon && !isAdmin)
            throw new ForbiddenOperationException("Las categorías comunes solo las puede borrar un administrador.");

        return await _incomeCategoryRepository.DeleteAsync(id);
    }
}
