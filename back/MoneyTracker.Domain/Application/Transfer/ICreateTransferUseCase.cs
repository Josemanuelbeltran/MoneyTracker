using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Transfer;

public interface ICreateTransferUseCase
{
    Task<TransferEntity> ExecuteAsync(int userId, int fromAccountId, int toAccountId, decimal amount, string? description, DateTime transferDate);
}
