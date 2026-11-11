using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Pocket;

public interface IUpdatePocketUseCase
{
    Task<PocketEntity?> ExecuteAsync(int id, string name, int userId);
}
