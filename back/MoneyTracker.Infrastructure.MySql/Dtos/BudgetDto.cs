namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class BudgetDto
{
    public int Id { get; set; }                  // id
    public int UserId { get; set; }              // user_id
    public int CategoryId { get; set; }          // category_id
    public decimal AmountLimit { get; set; }     // amount_limit (decimal(10,2))
    public DateOnly PeriodStart { get; set; }    // period_start (date)
    public DateOnly PeriodEnd { get; set; }      // period_end (date)
    public string? Notes { get; set; }           // notes (varchar 255, nullable)
    public DateTime CreatedAt { get; set; }      // created_at
    public DateTime UpdatedAt { get; set; }      // updated_at
}