namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class UserDto
{
    public int Id { get; set; }                       // PK
    public string Name { get; set; } = string.Empty;  // NOT NULL (varchar 50)
    
    public string Email { get; set; } = string.Empty; // NOT NULL (varchar 100)
    public string PasswordHash { get; set; } = string.Empty; // NOT NULL (varchar 255)
    public string? PhoneNumber { get; set; }          // NULL (varchar 15)
    public bool? VerifiedPhone { get; set; } = false;  // tinyint(1)
    public bool IsAdmin { get; set; }                 // is_admin tinyint(1)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // datetime
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // datetime
}