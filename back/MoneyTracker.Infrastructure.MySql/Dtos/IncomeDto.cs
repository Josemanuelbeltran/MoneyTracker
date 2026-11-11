namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class IncomeDto
{
    public int Id { get; set; }                  // id
    public int UserId { get; set; }              // user_id
    public int AccountId { get; set; }           // account_id
    public int CategoryId { get; set; }          // category_id
    public decimal Amount { get; set; }          // amount (decimal(10,2))
    public DateOnly IncomeDate { get; set; }     // income_date (date)
    public string? Description { get; set; }     // description (nullable)
    public bool IsRecurring { get; set; }        // is_recurring (fijo/recurrente)
    public DateTime CreatedAt { get; set; }      // created_at
    public DateTime UpdatedAt { get; set; }      // updated_at
}