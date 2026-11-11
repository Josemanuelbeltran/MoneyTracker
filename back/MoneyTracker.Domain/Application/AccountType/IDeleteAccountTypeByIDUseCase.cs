namespace MoneyTracker.Domain.Application.AccountType;

public interface IDeleteAccountTypeByIdUseCase
{
    Task<bool> ExecuteAsync(int id);
}