using MoneyTracker.Domain.Application.ExpenseCategory;
using MoneyTracker.Domain.Entities.Categories;
using MoneyTracker.Domain.Exceptions;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace ClassLibrary1MoneyTracker.Application.UseCases.ExpenseCategories;

public class CreateExpenseCategoryUseCase : ICreateExpenseCategoryUseCase
{
    private readonly IExpenseCategoryRepository _expenseCategoryRepository;
    private readonly IExpenseCategoryQueryService _query;

    public CreateExpenseCategoryUseCase(
        IExpenseCategoryRepository expenseCategoryRepository,
        IExpenseCategoryQueryService query)
    {
        _expenseCategoryRepository = expenseCategoryRepository;
        _query = query;
    }

    public async Task<ExpenseCategoryEntity> ExecuteAsync(int? parentId, string name, bool isCommon, int userId, bool isAdmin)
    {
        if (isCommon && !isAdmin)
            throw new ForbiddenOperationException("Solo un administrador puede crear categorías comunes.");

        // El padre tiene que ser una categoría que el usuario vea (común o suya).
        if (parentId.HasValue && !await _query.ExistsAsync(parentId.Value, userId))
            throw new ArgumentException($"ParentId {parentId} no existe.");

        // Común => sin dueño (user_id NULL); si no, la categoría es del usuario.
        return await _expenseCategoryRepository.CreateAsync(parentId, name.Trim(), isCommon ? null : userId);
    }
}
