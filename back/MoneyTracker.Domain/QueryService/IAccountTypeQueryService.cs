using MoneyTracker.Domain.Entities.types;

namespace MoneyTracker.Domain.QueryService;

public interface IAccountTypeQueryService
{
    Task<List<AccountTypeEntity>> GetAllAsync();
}
