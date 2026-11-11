using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.QueryService;

public interface IExpenseReimbursementQueryService
{
    /// <summary>Reembolso del usuario. Devuelve null si no existe o no es suyo.</summary>
    Task<ExpenseReimbursementEntity?> GetByIdAsync(int id, int userId);

    /// <summary>Reembolsos colgados de un gasto: uno por persona que te debe su parte.</summary>
    Task<List<ExpenseReimbursementEntity>> GetByExpenseIdAsync(int expenseId, int userId);

    Task<List<ExpenseReimbursementEntity>> GetByUserAsync(int userId);
}
