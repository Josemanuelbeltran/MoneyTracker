using MoneyTracker.Domain.Application.Income;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.Income;

public class GetIncomeByIdUseCase : IGetIncomeByIdUseCase
{
    private readonly IIncomeQueryService _incomeQueryService;

    public GetIncomeByIdUseCase(IIncomeQueryService incomeQueryService)
    {
        _incomeQueryService = incomeQueryService;
    }

    public Task<IncomeEntity?> ExecuteAsync(int id, int userId)
    {
        return _incomeQueryService.GetByIdAsync(id, userId);
    }
}
