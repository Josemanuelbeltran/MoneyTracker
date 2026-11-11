namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class ExpenseReimbursementDto
{
    public int Id { get; set; }                  // id
    public int UserId { get; set; }              // user_id
    public int ExpenseId { get; set; }           // expense_id
    public decimal Amount { get; set; }          // amount (decimal(10,2))
    public int ToAccountId { get; set; }         // to_account_id
    public DateTime? SettledAt { get; set; }     // settled_at (NULL = pendiente)
    public string? Debtor { get; set; }          // debtor (nullable)
    public int? PersonId { get; set; }           // person_id (nullable)
    public DateTime CreatedAt { get; set; }      // created_at
    public DateTime UpdatedAt { get; set; }      // updated_at
}
