using MoneyTracker.Domain.Entities.types;

namespace MoneyTracker.Domain.Application.AccountType;

public interface IGetAccountTypesUseCase
{
    Task<List<AccountTypeEntity>> ExecuteAsync();
}
