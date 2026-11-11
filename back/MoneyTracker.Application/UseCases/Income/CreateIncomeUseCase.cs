using MoneyTracker.Domain.Application.Income;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Income;

public class CreateIncomeUseCase : ICreateIncomeUseCase
{
    private readonly IIncomeRepository _incomeRepository;

    public CreateIncomeUseCase(IIncomeRepository incomeRepository)
    {
        _incomeRepository = incomeRepository;
    }

    public Task<IncomeEntity> ExecuteAsync(IncomeEntity income)
    {
        return _incomeRepository.CreateAsync(income);
    }
}
