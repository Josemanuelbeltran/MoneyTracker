using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Account;

public interface IUpdateAccountUseCase
{
    Task<AccountEntity?> ExecuteAsync(string? name, int? accountTypeId, decimal? balance, int id, int userId);
}
