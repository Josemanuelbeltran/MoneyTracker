using MoneyTracker.Domain.Entities.types;

namespace MoneyTracker.Domain.Application.AccountType;

public interface ICreateAccountTypeUseCase
{
    Task<AccountTypeEntity> ExecuteAsync(string name);
}