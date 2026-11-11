
namespace MoneyTracker.Domain.Application.AccountType;

public interface IUpdateAccountTypeUseCase
{
    Task<bool> ExecuteAsync(int id, string name);
}