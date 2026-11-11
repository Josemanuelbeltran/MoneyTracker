using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Income;

public interface IGetIncomeByIdUseCase
{
    Task<IncomeEntity?> ExecuteAsync(int id, int userId);
}
