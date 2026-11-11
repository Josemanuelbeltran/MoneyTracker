using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Account;

public interface IGetAccountByIdUseCase
{
    Task<AccountEntity?> ExecuteAsync(int id, int userId);
}
