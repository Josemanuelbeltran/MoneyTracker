using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Pocket;

public interface ICreatePocketUseCase
{
    Task<PocketEntity> ExecuteAsync(int userId, int accountId, string name, decimal balance);
}
