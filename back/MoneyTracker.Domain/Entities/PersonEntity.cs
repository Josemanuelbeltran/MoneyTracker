namespace MoneyTracker.Domain.Entities;

/// <summary>
/// Persona con la que el usuario comparte gastos (quien le hace el bizum).
/// Es de cada usuario: no se comparten agendas entre cuentas.
/// </summary>
public sealed class PersonEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;

    public PersonEntity() { }

    public PersonEntity(int userId, string name)
    {
        UserId = userId;
        Name = name;
    }
}
