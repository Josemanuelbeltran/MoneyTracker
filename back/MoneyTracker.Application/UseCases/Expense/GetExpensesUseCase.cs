using MoneyTracker.Domain.Application.Expense;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.Expense;

public class GetExpensesUseCase : IGetExpensesUseCase
{
    private readonly IExpenseQueryService _expenseQueryService;

    public GetExpensesUseCase(IExpenseQueryService expenseQueryService)
    {
        _expenseQueryService = expenseQueryService;
    }

    public Task<List<ExpenseEntity>> ExecuteAsync(int userId)
    {
        return _expenseQueryService.GetByUserAsync(userId);
    }
}
