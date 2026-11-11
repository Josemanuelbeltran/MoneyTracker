using MoneyTracker.Domain.Application.Income;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.Income;

public class GetIncomesUseCase : IGetIncomesUseCase
{
    private readonly IIncomeQueryService _incomeQueryService;

    public GetIncomesUseCase(IIncomeQueryService incomeQueryService)
    {
        _incomeQueryService = incomeQueryService;
    }

    public Task<List<IncomeEntity>> ExecuteAsync(int userId)
    {
        return _incomeQueryService.GetByUserAsync(userId);
    }
}
