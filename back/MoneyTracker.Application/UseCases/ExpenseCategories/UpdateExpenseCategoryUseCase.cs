// Application/UseCases/ExpenseCategory/UpdateExpenseCategoryUseCase.cs

using MoneyTracker.Domain.Application.ExpenseCategory;
using MoneyTracker.Domain.Entities.Categories;
using MoneyTracker.Domain.Exceptions;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.ExpenseCategories;

public class UpdateExpenseCategoryUseCase : IUpdateExpenseCategoryUseCase
{
    private readonly IExpenseCategoryRepository _repo;
    private readonly IExpenseCategoryQueryService _query;

    public UpdateExpenseCategoryUseCase(IExpenseCategoryRepository repo, IExpenseCategoryQueryService query)
    {
        _repo = repo;
        _query = query;
    }

    public async Task<ExpenseCategoryEntity?> ExecuteAsync(int id, string? name, int? parentId, bool? isCommon, int userId, bool isAdmin)
    {
        // Solo se ven (y por tanto solo se editan) las comunes y las propias.
        var current = await _query.GetByIdAsync(id, userId);
        if (current is null) return null;

        // Las comunes son de solo lectura salvo para el administrador.
        if (current.IsCommon && !isAdmin)
            throw new ForbiddenOperationException("Las categorías comunes solo las puede editar un administrador.");

        var newName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();

        var changedName   = newName is not null && newName != current.Name;
        var changedParent = parentId != current.ParentId;

        if (changedName) current.Name = newName!;

        if (changedParent)
        {
            if (parentId == id)
                throw new InvalidOperationException("Una categoría no puede ser su propio padre.");

            if (parentId.HasValue && !await _query.ExistsAsync(parentId.Value, userId))
                throw new InvalidOperationException($"ParentId {parentId} no existe.");

            current.ParentId = parentId; // null => raíz
        }

        // Cambiar común <-> propia es cosa del administrador.
        var changedOwner = false;
        if (isCommon is bool wantCommon && wantCommon != current.IsCommon)
        {
            if (!isAdmin)
                throw new ForbiddenOperationException("Solo un administrador puede marcar una categoría como común.");

            // Al dejar de ser común pasa a ser del administrador que la cambia.
            current.UserId = wantCommon ? null : userId;
            changedOwner = true;
        }

        // No-Op: nada cambió → no persistimos
        if (!changedName && !changedParent && !changedOwner) return current;

        return await _repo.UpdateAsync(current);
    }

}
