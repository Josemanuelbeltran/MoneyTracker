using MoneyTracker.Domain.Entities.Categories;

namespace MoneyTracker.Domain.QueryService;

/// <summary>
/// Lectura de categorías de ingreso: comunes (user_id NULL) + las del usuario.
/// </summary>
public interface IIncomeCategoryQueryService
{
    Task<List<IncomeCategoryEntity>> GetAllAsync(int userId);

    Task<IncomeCategoryEntity?> GetByIdAsync(int id, int userId);
}
