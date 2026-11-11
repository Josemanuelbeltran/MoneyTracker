namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class CategoryBudgetDto
{
    public int Id { get; set; }                  // id
    public int UserId { get; set; }              // user_id
    public int CategoryId { get; set; }          // category_id
    public decimal MonthlyLimit { get; set; }    // monthly_limit (decimal(10,2))
    public DateTime CreatedAt { get; set; }      // created_at
    public DateTime UpdatedAt { get; set; }      // updated_at
}
