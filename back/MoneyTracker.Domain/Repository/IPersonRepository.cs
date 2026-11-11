using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Repository;

public interface IPersonRepository
{
    Task<PersonEntity> CreateAsync(PersonEntity entity);

    /// <summary>Renombra una persona del usuario. Si no es suya, no toca nada.</summary>
    Task<bool> UpdateAsync(int id, int userId, string name);

    /// <summary>Borra una persona del usuario. Los reembolsos se quedan con el nombre.</summary>
    Task<bool> DeleteAsync(int id, int userId);

    /// <summary>Busca por nombre exacto dentro de la agenda del usuario.</summary>
    Task<PersonEntity?> GetByNameAsync(int userId, string name);
}
