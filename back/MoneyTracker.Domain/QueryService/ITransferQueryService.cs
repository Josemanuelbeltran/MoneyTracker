using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.QueryService;

public interface ITransferQueryService
{
    Task<List<TransferEntity>> GetByUserAsync(int userId);
}
