using MoneyTracker.Domain.Application.IncomeCategory;
using MoneyTracker.Domain.Entities.Categories;
using MoneyTracker.Domain.Exceptions;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.IncomeCategories;

public class CreateIncomeCategoryUseCase : ICreateIncomeCategoryUseCase
{
    private readonly IIncomeCategoryRepository _incomeCategoryRepository;
    private readonly IIncomeCategoryQueryService _query;

    public CreateIncomeCategoryUseCase(
        IIncomeCategoryRepository incomeCategoryRepository,
        IIncomeCategoryQueryService query)
    {
        _incomeCategoryRepository = incomeCategoryRepository;
        _query = query;
    }

    public async Task<IncomeCategoryEntity> ExecuteAsync(int? parentId, string name, bool isCommon, int userId, bool isAdmin)
    {
        if (isCommon && !isAdmin)
            throw new ForbiddenOperationException("Solo un administrador puede crear categorías comunes.");

        // El padre tiene que ser una categoría que el usuario vea (común o suya).
        if (parentId.HasValue && await _query.GetByIdAsync(parentId.Value, userId) is null)
            throw new ArgumentException($"ParentId {parentId} no existe.");

        // Común => sin dueño (user_id NULL); si no, la categoría es del usuario.
        return await _incomeCategoryRepository.CreateAsync(parentId, name.Trim(), isCommon ? null : userId);
    }
}
