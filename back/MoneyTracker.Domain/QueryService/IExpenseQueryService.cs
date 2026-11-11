using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.QueryService;

public interface IExpenseQueryService
{
    /// <summary>Gasto del usuario. Devuelve null si no existe o no es suyo.</summary>
    Task<ExpenseEntity?> GetByIdAsync(int id, int userId);

    Task<List<ExpenseEntity>> GetByUserAsync(int userId);
}
