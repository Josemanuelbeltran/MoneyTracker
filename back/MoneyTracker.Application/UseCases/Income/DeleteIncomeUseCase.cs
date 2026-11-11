using MoneyTracker.Domain.Application.Income;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Income;

public class DeleteIncomeUseCase : IDeleteIncomeUseCase
{
    private readonly IIncomeRepository _incomeRepository;

    public DeleteIncomeUseCase(IIncomeRepository incomeRepository)
    {
        _incomeRepository = incomeRepository;
    }

    public Task<bool> ExecuteAsync(int id, int userId)
    {
        return _incomeRepository.DeleteAsync(id, userId);
    }
}
