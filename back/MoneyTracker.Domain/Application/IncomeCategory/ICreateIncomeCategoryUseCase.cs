using MoneyTracker.Domain.Entities.Categories;

namespace MoneyTracker.Domain.Application.IncomeCategory;

public interface ICreateIncomeCategoryUseCase
{
    /// <summary>
    /// Crea una categoría. Con <paramref name="isCommon"/> a true la crea como común
    /// (visible para todos), lo que solo puede hacer un administrador.
    /// </summary>
    Task<IncomeCategoryEntity> ExecuteAsync(int? parentId, string name, bool isCommon, int userId, bool isAdmin);
}
