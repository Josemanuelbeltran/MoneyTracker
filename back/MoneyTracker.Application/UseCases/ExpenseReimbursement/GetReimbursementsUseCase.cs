using MoneyTracker.Domain.Application.ExpenseReimbursement;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.ExpenseReimbursement;

public class GetReimbursementsUseCase : IGetReimbursementsUseCase
{
    private readonly IExpenseReimbursementQueryService _queryService;

    public GetReimbursementsUseCase(IExpenseReimbursementQueryService queryService)
    {
        _queryService = queryService;
    }

    public Task<List<ExpenseReimbursementEntity>> ExecuteAsync(int userId)
    {
        return _queryService.GetByUserAsync(userId);
    }
}
