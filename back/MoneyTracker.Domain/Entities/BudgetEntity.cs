namespace MoneyTracker.Domain.Entities;

public sealed class BudgetEntity
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int CategoryId { get; private set; }
    public decimal AmountLimit { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public string? Notes { get; private set; }

    private BudgetEntity() { }

    public BudgetEntity(int userId, int categoryId, decimal amountLimit, DateOnly periodStart, DateOnly periodEnd, string? notes = null)
    {
        UserId = userId;
        CategoryId = categoryId;
        AmountLimit = amountLimit;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        Notes = notes;
    }
}
