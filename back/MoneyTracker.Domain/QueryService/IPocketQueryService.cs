using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.QueryService;

public interface IPocketQueryService
{
    Task<List<PocketEntity>> GetByUserAsync(int userId);
}
