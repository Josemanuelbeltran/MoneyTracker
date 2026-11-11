using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Account;

public interface IGetAccountsUseCase
{
    Task<List<AccountEntity>> ExecuteAsync(int userId);
}
