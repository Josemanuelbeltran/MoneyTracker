namespace MoneyTracker.Domain.Application.ExpenseReimbursement;

public interface IDeleteReimbursementUseCase
{
    Task<bool> ExecuteAsync(int id, int userId);
}
