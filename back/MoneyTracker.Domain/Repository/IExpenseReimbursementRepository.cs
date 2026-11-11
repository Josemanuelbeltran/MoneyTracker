using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Repository;

public interface IExpenseReimbursementRepository
{
    Task<ExpenseReimbursementEntity> CreateAsync(ExpenseReimbursementEntity entity);

    /// <summary>Marca el reembolso como cobrado. Si no es del usuario, no toca nada.</summary>
    Task<bool> SettleAsync(int id, int userId, DateTime settledAt);

    /// <summary>Vuelve a dejarlo pendiente (por si lo marcaste cobrado sin querer).</summary>
    Task<bool> UnsettleAsync(int id, int userId);

    /// <summary>Cambia importe, cuenta de destino y persona. Solo toca lo que no sea null.</summary>
    Task<bool> UpdateAsync(int id, int userId, decimal? amount, int? toAccountId, int? personId, string? debtor);

    /// <summary>Borra el reembolso del usuario. Si no es suyo, no borra nada.</summary>
    Task<bool> DeleteAsync(int id, int userId);
}
