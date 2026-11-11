namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class PersonDto
{
    public int Id { get; set; }               // id
    public int UserId { get; set; }           // user_id
    public string Name { get; set; } = string.Empty; // name
    public DateTime CreatedAt { get; set; }   // created_at
    public DateTime UpdatedAt { get; set; }   // updated_at
}
