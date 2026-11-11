using MoneyTracker.Domain.Application.Income;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Income;

public class UpdateIncomeUseCase : IUpdateIncomeUseCase
{
    private readonly IIncomeRepository _incomeRepository;

    public UpdateIncomeUseCase(IIncomeRepository incomeRepository)
    {
        _incomeRepository = incomeRepository;
    }

    public Task<IncomeEntity> ExecuteAsync(IncomeEntity income)
    {
        return _incomeRepository.UpdateAsync(income);
    }
}
