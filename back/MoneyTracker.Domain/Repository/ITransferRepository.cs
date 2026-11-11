using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Repository;

public interface ITransferRepository
{
    Task<TransferEntity> CreateAsync(TransferEntity entity);

    /// <summary>Borra una transferencia del usuario. Si no es suya, no borra nada.</summary>
    Task<bool> DeleteAsync(int id, int userId);
}
