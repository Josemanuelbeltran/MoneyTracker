namespace MoneyTracker.Domain.Entities;

public sealed class ExpenseEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
    public bool IsRecurring { get; set; }
    public int? PocketId { get; set; }

    /// <summary>Mascota a la que va el gasto (id de PetRecording). NULL = no es un gasto de mascota.</summary>
    public int? PetId { get; set; }

    /// <summary>Importe completo del gasto compartido. NULL = el gasto no es parcial.</summary>
    public decimal? TotalAmount { get; set; }

    /// <summary>Entre cuántas partes se divide. NULL = el gasto no es parcial.</summary>
    public int? SplitParts { get; set; }

    /// <summary>Identificador común a las N cuotas de un mismo prorrateo. NULL = no prorrateado.</summary>
    public string? SpreadGroup { get; set; }

    /// <summary>Nº de cuota dentro del prorrateo, empezando en 1.</summary>
    public int? SpreadIndex { get; set; }

    /// <summary>Total de cuotas del prorrateo.</summary>
    public int? SpreadCount { get; set; }

    /// <summary>El gasto es parcial (solo se paga una de varias partes).</summary>
    public bool IsSplit => SplitParts is > 1;

    /// <summary>El gasto forma parte de un prorrateo en varios meses.</summary>
    public bool IsSpread => SpreadGroup is not null;

    public ExpenseEntity() { }

    public ExpenseEntity(int userId, int accountId, int categoryId, decimal amount, DateTime expenseDate, string? description = null)
    {
        UserId = userId;
        AccountId = accountId;
        CategoryId = categoryId;
        Amount = amount;
        ExpenseDate = expenseDate;
        Description = description;
    }
}
