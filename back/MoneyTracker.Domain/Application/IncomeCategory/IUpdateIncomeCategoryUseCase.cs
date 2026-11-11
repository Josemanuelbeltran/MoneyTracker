using MoneyTracker.Domain.Entities.Categories;

namespace MoneyTracker.Domain.Application.IncomeCategory;

public interface IUpdateIncomeCategoryUseCase
{
    /// <summary>
    /// Renombra una categoría. <paramref name="isCommon"/> (solo admin) la pasa de común
    /// a propia y al revés; null = no se toca la propiedad.
    /// </summary>
    Task<IncomeCategoryEntity?> ExecuteAsync(int id, string name, bool? isCommon, int userId, bool isAdmin);
}
