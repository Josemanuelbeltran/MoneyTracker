using MoneyTracker.Domain.Application.ExpenseReimbursement;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.ExpenseReimbursement;

public class DeleteReimbursementUseCase : IDeleteReimbursementUseCase
{
    private readonly IExpenseReimbursementRepository _repository;
    private readonly IExpenseReimbursementQueryService _queryService;
    private readonly IExpenseQueryService _expenseQueryService;
    private readonly IExpenseRepository _expenseRepository;

    public DeleteReimbursementUseCase(
        IExpenseReimbursementRepository repository,
        IExpenseReimbursementQueryService queryService,
        IExpenseQueryService expenseQueryService,
        IExpenseRepository expenseRepository)
    {
        _repository = repository;
        _queryService = queryService;
        _expenseQueryService = expenseQueryService;
        _expenseRepository = expenseRepository;
    }

    public async Task<bool> ExecuteAsync(int id, int userId)
    {
        var reimbursement = await _queryService.GetByIdAsync(id, userId);
        if (reimbursement is null)
            return false;

        // Quitar el reembolso significa "al final lo pagué yo entero", así que ese dinero
        // pasa a ser TU gasto: el importe del gasto sube hasta el total. Si no lo
        // subiéramos, el gasto seguiría diciendo 42 cuando de la cuenta y del pocket
        // salieron 60, y ahí es donde dejaba de cuadrar.
        var expense = await _expenseQueryService.GetByIdAsync(reimbursement.ExpenseId, userId);
        if (expense is not null)
        {
            expense.Amount += reimbursement.Amount;
            // Ya no hay nada compartido: lo pagas tú todo.
            expense.TotalAmount = null;
            expense.SplitParts = null;
            await _expenseRepository.UpdateAsync(expense);
        }

        return await _repository.DeleteAsync(id, userId);
    }
}
