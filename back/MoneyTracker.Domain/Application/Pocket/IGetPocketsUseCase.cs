using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Pocket;

public interface IGetPocketsUseCase
{
    Task<List<PocketEntity>> ExecuteAsync(int userId);
}
