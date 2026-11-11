using MoneyTracker.Domain.Application.Expense;
using MoneyTracker.Domain.Integration;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Expense;

public class DeleteExpenseUseCase : IDeleteExpenseUseCase
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseQueryService _expenseQueryService;
    private readonly IExpenseReimbursementQueryService _reimbursementQueryService;
    private readonly IPocketRepository _pocketRepository;
    private readonly IPetExpenseNotifier _petNotifier;

    public DeleteExpenseUseCase(
        IExpenseRepository expenseRepository,
        IExpenseQueryService expenseQueryService,
        IExpenseReimbursementQueryService reimbursementQueryService,
        IPocketRepository pocketRepository,
        IPetExpenseNotifier petNotifier)
    {
        _expenseRepository = expenseRepository;
        _expenseQueryService = expenseQueryService;
        _reimbursementQueryService = reimbursementQueryService;
        _pocketRepository = pocketRepository;
        _petNotifier = petNotifier;
    }

    public async Task<bool> ExecuteAsync(int id, int userId)
    {
        // Si el gasto salió de un pocket, al borrarlo le devolvemos lo que salió.
        var expense = await _expenseQueryService.GetByIdAsync(id, userId);

        // Del pocket salió el TOTAL (tu parte más lo que adelantaste), así que hay que
        // devolver las dos cosas o el pocket se queda corto. El reembolso se lee antes
        // de borrar: la FK lo borra en cascada junto con el gasto.
        var reimbursements = await _reimbursementQueryService.GetByExpenseIdAsync(id, userId);

        var deleted = await _expenseRepository.DeleteAsync(id, userId);

        if (deleted && expense?.PocketId is int pid)
        {
            var giveBack = expense.Amount + reimbursements.Sum(r => r.Amount);
            try
            {
                await _pocketRepository.MoveAsync(null, pid, giveBack, userId);
            }
            catch (KeyNotFoundException)
            {
                // El pocket ya no existe (fue borrado): no hay nada que devolver.
            }
        }

        // Era de mascota: que desaparezca también de PetRecording.
        if (deleted && expense?.PetId is not null)
        {
            try
            {
                await _petNotifier.ExpenseDeletedAsync(id);
            }
            catch
            {
                // El notificador ya traga y registra sus propios errores.
            }
        }

        return deleted;
    }
}
