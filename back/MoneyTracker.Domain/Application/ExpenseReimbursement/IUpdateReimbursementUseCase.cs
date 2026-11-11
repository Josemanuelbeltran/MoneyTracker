namespace MoneyTracker.Domain.Application.ExpenseReimbursement;

public interface IUpdateReimbursementUseCase
{
    /// <summary>
    /// Cambia lo pactado: cuánto te deben, en qué cuenta te lo devuelven (el bizum que
    /// al final es efectivo) y quién. Solo se toca lo que llegue informado.
    /// Si el gasto salía de un pocket, la diferencia de importe se ajusta en ese pocket.
    /// </summary>
    Task<bool> ExecuteAsync(int id, int userId, decimal? amount, int? toAccountId, int? personId, string? debtor);
}
