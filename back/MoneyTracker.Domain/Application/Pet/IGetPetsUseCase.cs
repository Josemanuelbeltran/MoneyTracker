using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Pet;

public interface IGetPetsUseCase
{
    /// <summary>Las mascotas del usuario: cada uno ve las que ha dado de alta.</summary>
    Task<List<PetEntity>> ExecuteAsync(int userId);
}
