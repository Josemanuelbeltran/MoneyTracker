namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class PetDto
{
    public int Id { get; set; }                       // id (el de PetRecording, a mano)
    public int UserId { get; set; }                   // user_id (de quien la creo)
    public string Name { get; set; } = string.Empty;  // name
}
