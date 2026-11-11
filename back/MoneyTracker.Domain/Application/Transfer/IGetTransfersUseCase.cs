using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Transfer;

public interface IGetTransfersUseCase
{
    Task<List<TransferEntity>> ExecuteAsync(int userId);
}
