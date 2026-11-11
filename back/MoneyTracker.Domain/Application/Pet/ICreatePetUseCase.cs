using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Pet;

public interface ICreatePetUseCase
{
    /// <summary>
    /// Da de alta la mascota con el id de PetRecording. Si el usuario ya tiene ese
    /// id y es la misma, devuelve la que hay; si lo tiene ocupado por otra, falla.
    /// </summary>
    Task<PetEntity> ExecuteAsync(int id, int userId, string name);
}
