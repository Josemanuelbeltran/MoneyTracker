using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.QueryService;

public interface IPersonQueryService
{
    Task<List<PersonEntity>> GetByUserAsync(int userId);
    Task<bool> ExistsAsync(int id, int userId);
}
