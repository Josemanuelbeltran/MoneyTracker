namespace MoneyTracker.Domain.Entities.types;

public sealed class AccountTypeEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public AccountTypeEntity() { }

    public AccountTypeEntity(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
