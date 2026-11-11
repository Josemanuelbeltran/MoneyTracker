namespace MoneyTracker.Domain.Entities;

/// <summary>
/// Mascota. Es un espejo de las de PetRecording: el <see cref="Id"/> es el suyo
/// (por eso se escribe a mano, no es autoincremental) y el nombre está aquí solo
/// para poder elegirla en el formulario del gasto.
///
/// Es de quien la da de alta: cada usuario ve las suyas. El id es único dentro
/// de cada usuario, no entre todos.
/// </summary>
public sealed class PetEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;

    public PetEntity() { }

    public PetEntity(int id, int userId, string name)
    {
        Id = id;
        UserId = userId;
        Name = name;
    }
}
