namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class AccountTypeDto
{
    public int Id { get; set; }              // id
    public string Name { get; set; } = "";   // name (varchar 50, unique)
}