namespace MoneyTracker.Domain.Entities;

public sealed class RealSpentMonthlyEntity
{
    public int UserId { get; private set; }
    public int CategoryId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal SpentAmount { get; private set; }

    private RealSpentMonthlyEntity() { }

    public RealSpentMonthlyEntity(int userId, int categoryId, int year, int month, decimal spentAmount)
    {
        UserId = userId;
        CategoryId = categoryId;
        Year = year;
        Month = month;
        SpentAmount = spentAmount;
    }
}
