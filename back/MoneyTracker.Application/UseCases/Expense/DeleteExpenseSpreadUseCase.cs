using MoneyTracker.Domain.Application.Expense;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.Expense;

public class DeleteExpenseSpreadUseCase : IDeleteExpenseSpreadUseCase
{
    private readonly IExpenseQueryService _expenseQueryService;
    private readonly IDeleteExpenseUseCase _deleteExpense;

    public DeleteExpenseSpreadUseCase(
        IExpenseQueryService expenseQueryService,
        IDeleteExpenseUseCase deleteExpense)
    {
        _expenseQueryService = expenseQueryService;
        _deleteExpense = deleteExpense;
    }

    public async Task<int> ExecuteAsync(int id, int userId)
    {
        var anchor = await _expenseQueryService.GetByIdAsync(id, userId)
                     ?? throw new KeyNotFoundException("Expense no existe");

        if (anchor.SpreadGroup is not string group)
            throw new ArgumentException("El gasto no está prorrateado: no forma parte de un grupo.");

        // Se reutiliza el borrado unitario para no duplicar la devolución al pocket.
        var all = await _expenseQueryService.GetByUserAsync(userId);
        var siblings = all.Where(e => e.SpreadGroup == group).ToList();

        var deleted = 0;
        foreach (var cuota in siblings)
        {
            if (await _deleteExpense.ExecuteAsync(cuota.Id, userId))
                deleted++;
        }

        return deleted;
    }
}
