namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class RealSpentMonthlyDto
{
    public int UserId { get; set; }             // user_id
    public int CategoryId { get; set; }         // category_id
    public int Year { get; set; }               // y
    public int Month { get; set; }              // m
    public decimal SpentAmount { get; set; }    // spent_amount
}