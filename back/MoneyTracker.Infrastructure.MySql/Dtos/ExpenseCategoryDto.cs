namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class ExpenseCategoryDto
{
    public int Id { get; set; }                 // id
    public string Name { get; set; } = "";      // name
    public int? ParentId { get; set; }          // parent_id
    public int? UserId { get; set; }            // user_id (NULL = categoría común)
    public DateTime CreatedAt { get; set; }     // created_at
    public DateTime UpdatedAt { get; set; }     // updated_at
}
