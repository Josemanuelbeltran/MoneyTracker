using MoneyTracker.Domain.Entities.Categories;

namespace MoneyTracker.Domain.QueryService;

/// <summary>
/// Lectura de categorías de gasto. Todos los métodos reciben el usuario porque
/// solo son visibles las comunes (user_id NULL) y las propias del usuario.
/// </summary>
public interface IExpenseCategoryQueryService
{
    Task<ExpenseCategoryEntity?> GetByIdWithChildrenAsync(int id, int userId);

    Task<List<ExpenseCategoryEntity>> GetAllTreeAsync(int userId);

    Task<ExpenseCategoryEntity?> GetByIdAsync(int id, int userId);

    Task<bool> ExistsAsync(int id, int userId);
}
