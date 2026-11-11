namespace MoneyTracker.Domain.Entities;

/// <summary>
/// Parte de un gasto compartido que el usuario adelantó y le tienen que devolver
/// (normalmente por bizum). El <see cref="ExpenseEntity.Amount"/> del gasto sigue
/// siendo su parte; esto es el dinero que salió de más de su cuenta.
/// </summary>
public sealed class ExpenseReimbursementEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }

    /// <summary>Gasto que se adelantó. Al borrarlo, el reembolso se borra con él.</summary>
    public int ExpenseId { get; set; }

    /// <summary>Lo que te deben = total del gasto − tu parte.</summary>
    public decimal Amount { get; set; }

    /// <summary>Cuenta donde te harán el bizum.</summary>
    public int ToAccountId { get; set; }

    /// <summary>NULL mientras esté pendiente; con fecha cuando ya lo has cobrado.</summary>
    public DateTime? SettledAt { get; set; }

    /// <summary>Quién te lo debe. Texto libre y opcional.</summary>
    public string? Debtor { get; set; }

    /// <summary>Persona de tu agenda que te lo debe. NULL si no la indicaste.</summary>
    public int? PersonId { get; set; }

    /// <summary>Todavía no te han devuelto el dinero.</summary>
    public bool IsPending => SettledAt is null;

    public ExpenseReimbursementEntity() { }

    public ExpenseReimbursementEntity(int userId, int expenseId, decimal amount, int toAccountId, string? debtor = null)
    {
        UserId = userId;
        ExpenseId = expenseId;
        Amount = amount;
        ToAccountId = toAccountId;
        Debtor = debtor;
    }
}
