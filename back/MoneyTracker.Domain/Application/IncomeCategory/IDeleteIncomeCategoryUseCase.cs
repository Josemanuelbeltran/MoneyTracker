namespace MoneyTracker.Domain.Application.IncomeCategory;

public interface IDeleteIncomeCategoryUseCase
{
    Task<bool> ExecuteAsync(int id, int userId, bool isAdmin);
}
