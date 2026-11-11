using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Repository;

public interface IPetRepository
{
    /// <summary>Alta con el id puesto a mano: es el de PetRecording, no lo genera la BD.</summary>
    Task<PetEntity> CreateAsync(PetEntity entity);
}
