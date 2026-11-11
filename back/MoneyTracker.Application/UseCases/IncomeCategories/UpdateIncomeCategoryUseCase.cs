using MoneyTracker.Domain.Application.IncomeCategory;
using MoneyTracker.Domain.Entities.Categories;
using MoneyTracker.Domain.Exceptions;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.IncomeCategories;

public class UpdateIncomeCategoryUseCase : IUpdateIncomeCategoryUseCase
{
    private readonly IIncomeCategoryRepository _incomeCategoryRepository;
    private readonly IIncomeCategoryQueryService _query;

    public UpdateIncomeCategoryUseCase(
        IIncomeCategoryRepository incomeCategoryRepository,
        IIncomeCategoryQueryService query)
    {
        _incomeCategoryRepository = incomeCategoryRepository;
        _query = query;
    }

    public async Task<IncomeCategoryEntity?> ExecuteAsync(int id, string name, bool? isCommon, int userId, bool isAdmin)
    {
        // Solo se ven (y por tanto solo se editan) las comunes y las propias.
        var current = await _query.GetByIdAsync(id, userId);
        if (current is null) return null;

        // Las comunes son de solo lectura salvo para el administrador.
        if (current.IsCommon && !isAdmin)
            throw new ForbiddenOperationException("Las categorías comunes solo las puede editar un administrador.");

        current.Name = name.Trim();

        // Cambiar común <-> propia es cosa del administrador.
        if (isCommon is bool wantCommon && wantCommon != current.IsCommon)
        {
            if (!isAdmin)
                throw new ForbiddenOperationException("Solo un administrador puede marcar una categoría como común.");

            // Al dejar de ser común pasa a ser del administrador que la cambia.
            current.UserId = wantCommon ? null : userId;
        }

        return await _incomeCategoryRepository.UpdateAsync(current);
    }
}
