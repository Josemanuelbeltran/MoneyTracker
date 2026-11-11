namespace MoneyTracker.Domain.Application.ExpenseReimbursement;

public interface ISettleReimbursementUseCase
{
    /// <summary>Marca como cobrado (o vuelve a dejar pendiente si <paramref name="settled"/> es false).</summary>
    Task<bool> ExecuteAsync(int id, int userId, bool settled);
}
