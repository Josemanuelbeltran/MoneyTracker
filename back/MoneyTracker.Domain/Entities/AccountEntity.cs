namespace MoneyTracker.Domain.Entities;


public sealed class AccountEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AccountTypeId { get; set; }
    public string Name { get; set; } = default!;
    public decimal Balance { get;  set; }

    public AccountEntity() { }

    public AccountEntity(int userId, int accountTypeId, string name, decimal balance = 0)
    {
        UserId = userId;
        AccountTypeId = accountTypeId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Balance = balance;
    }
}
