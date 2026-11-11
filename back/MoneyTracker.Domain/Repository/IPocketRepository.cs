using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Repository;

public interface IPocketRepository
{
    Task<PocketEntity> CreateAsync(PocketEntity entity);

    /// <summary>Renombra un pocket del usuario. Devuelve null si no existe o no es suyo.</summary>
    Task<PocketEntity?> UpdateAsync(int id, string name, int userId);

    /// <summary>Borra un pocket del usuario. Si no es suyo, no borra nada.</summary>
    Task<bool> DeleteAsync(int id, int userId);

    /// <summary>Ajusta el saldo de los pockets del usuario implicados en un movimiento (atómico).</summary>
    Task MoveAsync(int? fromPocketId, int? toPocketId, decimal amount, int userId);
}
