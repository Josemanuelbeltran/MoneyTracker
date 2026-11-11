using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.QueryService;

/// <summary>
/// Lectura de la tabla auxiliar de mascotas. Cada usuario ve las suyas: los ids
/// son los de PetRecording y son únicos dentro de cada usuario.
/// </summary>
public interface IPetQueryService
{
    Task<PetEntity?> GetByIdAsync(int id, int userId);

    Task<List<PetEntity>> GetByUserAsync(int userId);
}
