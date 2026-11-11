namespace MoneyTracker.Domain.Application.Account;

public interface IDeleteAccountUseCase
{
    Task<bool?> ExecuteAsync(int id, int userId);
    
}
