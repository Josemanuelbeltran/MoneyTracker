namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class AccountDto
{
    public int Id { get; set; }                  // id
    public int UserId { get; set; }              // user_id
    public int AccountTypeId { get; set; }       // account_type_id
    public string Name { get; set; } = "";       // name (varchar 100)
    public decimal Balance { get; set; }         // balance (decimal(10,2))
    public DateTime CreatedAt { get; set; }      // created_at
    public DateTime UpdatedAt { get; set; }      // updated_at
}