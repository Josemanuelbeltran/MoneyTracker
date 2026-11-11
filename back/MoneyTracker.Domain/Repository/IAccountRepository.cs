using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Repository;

public interface IAccountRepository
{
    Task<AccountEntity?> CreateAsync(AccountEntity entity);

    /// <summary>Borra una cuenta del usuario. Si no es suya, no borra nada.</summary>
    public Task<bool?> DeleteAsync(int id, int userId);

    /// <summary>Actualiza una cuenta del usuario (la entidad lleva su UserId).</summary>
    public Task<AccountEntity?> UpdateAsync(AccountEntity entity);

}
