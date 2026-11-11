namespace MoneyTracker.Domain.Entities;

public sealed class PocketEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AccountId { get; set; }
    public string Name { get; set; } = default!;
    public decimal Balance { get; set; }

    public PocketEntity() { }

    public PocketEntity(int userId, int accountId, string name, decimal balance = 0)
    {
        UserId = userId;
        AccountId = accountId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Balance = balance;
    }
}
