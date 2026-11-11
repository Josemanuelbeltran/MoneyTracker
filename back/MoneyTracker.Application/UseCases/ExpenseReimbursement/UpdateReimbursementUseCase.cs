using MoneyTracker.Domain.Application.ExpenseReimbursement;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.ExpenseReimbursement;

public class UpdateReimbursementUseCase : IUpdateReimbursementUseCase
{
    private readonly IExpenseReimbursementRepository _repository;
    private readonly IExpenseReimbursementQueryService _queryService;
    private readonly IExpenseQueryService _expenseQueryService;
    private readonly IPocketRepository _pocketRepository;

    public UpdateReimbursementUseCase(
        IExpenseReimbursementRepository repository,
        IExpenseReimbursementQueryService queryService,
        IExpenseQueryService expenseQueryService,
        IPocketRepository pocketRepository)
    {
        _repository = repository;
        _queryService = queryService;
        _expenseQueryService = expenseQueryService;
        _pocketRepository = pocketRepository;
    }

    public async Task<bool> ExecuteAsync(int id, int userId, decimal? amount, int? toAccountId, int? personId, string? debtor)
    {
        var current = await _queryService.GetByIdAsync(id, userId);
        if (current is null)
            return false;

        if (amount is decimal newAmount)
        {
            if (newAmount <= 0)
                throw new ArgumentException("Lo que te deben debe ser mayor que 0.");

            // El adelanto salió del pocket del gasto, así que si cambia lo que te deben
            // hay que ajustar ese pocket por la diferencia.
            var delta = newAmount - current.Amount;
            if (delta != 0)
            {
                var expense = await _expenseQueryService.GetByIdAsync(current.ExpenseId, userId);
                if (expense?.PocketId is int pocketId)
                {
                    if (delta > 0)
                        await _pocketRepository.MoveAsync(pocketId, null, delta, userId);
                    else
                        await _pocketRepository.MoveAsync(null, pocketId, -delta, userId);
                }
            }
        }

        return await _repository.UpdateAsync(id, userId, amount, toAccountId, personId, debtor);
    }
}
