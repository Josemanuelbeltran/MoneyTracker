using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Income;

public interface IUpdateIncomeUseCase
{
    Task<IncomeEntity> ExecuteAsync(IncomeEntity income);
}
