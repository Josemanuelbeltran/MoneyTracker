using MoneyTracker.Domain.Application.Expense;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.Expense;

public class GetExpenseByIdUseCase : IGetExpenseByIdUseCase
{
    private readonly IExpenseQueryService _expenseQueryService;
    public GetExpenseByIdUseCase(IExpenseQueryService expenseQueryService)
    {
        _expenseQueryService = expenseQueryService;
    }
    public Task<ExpenseEntity?> ExecuteAsync(int id, int userId)
    {
        return _expenseQueryService.GetByIdAsync(id, userId);
    }
}
