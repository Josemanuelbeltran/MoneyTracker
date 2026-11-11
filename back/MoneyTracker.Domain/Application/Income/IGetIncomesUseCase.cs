using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Income;

public interface IGetIncomesUseCase
{
    Task<List<IncomeEntity>> ExecuteAsync(int userId);
}
