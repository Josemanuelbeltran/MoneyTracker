namespace MoneyTracker.Domain.Entities;

public sealed class TransferEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransferDate { get; set; }
    public string? Description { get; set; }

    public TransferEntity() { }

    public TransferEntity(int userId, int fromAccountId, int toAccountId, decimal amount, DateTime transferDate, string? description = null)
    {
        UserId = userId;
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        Amount = amount;
        TransferDate = transferDate;
        Description = description;
    }
}
