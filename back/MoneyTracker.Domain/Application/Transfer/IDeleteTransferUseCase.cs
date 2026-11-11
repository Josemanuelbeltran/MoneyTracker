namespace MoneyTracker.Domain.Application.Transfer;

public interface IDeleteTransferUseCase
{
    Task<bool> ExecuteAsync(int id, int userId);
}
