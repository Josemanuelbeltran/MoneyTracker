namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class TransferDto
{
    public int Id { get; set; }                  // id
    public int UserId { get; set; }              // user_id
    public int FromAccountId { get; set; }       // from_account_id
    public int ToAccountId { get; set; }         // to_account_id
    public decimal Amount { get; set; }          // amount (decimal(10,2))
    public DateTime TransferDate { get; set; }   // transfer_date (datetime)
    public string? Description { get; set; }     // description (nullable)
    public DateTime CreatedAt { get; set; }      // created_at
    public DateTime UpdatedAt { get; set; }      // updated_at
}
