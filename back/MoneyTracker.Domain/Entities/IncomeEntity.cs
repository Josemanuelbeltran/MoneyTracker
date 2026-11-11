namespace MoneyTracker.Domain.Entities;

public sealed class IncomeEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly IncomeDate { get; set; }
    public string? Description { get; set; }
    public bool IsRecurring { get; set; }

    public IncomeEntity() { }

    public IncomeEntity(int userId, int accountId, int categoryId, decimal amount, DateOnly incomeDate, string? description = null)
    {
        UserId = userId;
        AccountId = accountId;
        CategoryId = categoryId;
        Amount = amount;
        IncomeDate = incomeDate;
        Description = description;
    }
}
