using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Account;

public interface ICreateAccountUseCase
{
    Task<AccountEntity?> ExecuteAsync(AccountEntity entity);
}