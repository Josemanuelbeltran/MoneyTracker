using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.User;

public interface ILoginUseCase
{
    Task<string?> ExecuteAsync(string username, string password);
}