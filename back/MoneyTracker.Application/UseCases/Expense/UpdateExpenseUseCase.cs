using MoneyTracker.Domain.Application.Expense;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Integration;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Expense;

public class UpdateExpenseUseCase : IUpdateExpenseUseCase
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseQueryService _expenseQueryService;
    private readonly IExpenseReimbursementQueryService _reimbursementQueryService;
    private readonly IPocketRepository _pocketRepository;
    private readonly IPetExpenseNotifier _petNotifier;

    public UpdateExpenseUseCase(
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

    public async Task<ExpenseEntity> ExecuteAsync(ExpenseEntity expense)
    {
        // Reajuste de pockets al editar: devolvemos al pocket anterior lo que salió
        // y descontamos del pocket nuevo (mismo patrón que Create/Delete).
        var old = await _expenseQueryService.GetByIdAsync(expense.Id, expense.UserId);

        // Del pocket salió el TOTAL, así que al cambiar de pocket tiene que moverse
        // entero: al viejo le vuelven tu parte MÁS el adelanto, y al nuevo se le quita
        // lo mismo. Mover solo tu parte dejaría el adelanto colgado en el pocket viejo.
        var reimbursements = await _reimbursementQueryService.GetByExpenseIdAsync(expense.Id, expense.UserId);
        var advance = reimbursements.Sum(r => r.Amount);

        if (old?.PocketId is int oldPid)
        {
            try
            {
                await _pocketRepository.MoveAsync(null, oldPid, old.Amount + advance, expense.UserId);
            }
            catch (KeyNotFoundException)
            {
                // El pocket anterior ya no existe: no hay nada que devolver.
            }
        }

        if (expense.PocketId is int newPid)
            await _pocketRepository.MoveAsync(newPid, null, expense.Amount + advance, expense.UserId);

        var updated = await _expenseRepository.UpdateAsync(expense);

        await NotifyPetRecordingAsync(updated, old);

        return updated;
    }

    /// <summary>
    /// Allí es un upsert por id, así que reenviar el gasto editado lo actualiza.
    /// Si ha dejado de ser de mascota (le quitas la mascota o lo cambias de
    /// categoría), hay que borrarlo allí o se queda un gasto fantasma.
    /// </summary>
    private async Task NotifyPetRecordingAsync(ExpenseEntity updated, ExpenseEntity? old)
    {
        try
        {
            if (updated.PetId is not null)
                await _petNotifier.ExpenseSavedAsync(updated);
            else if (old?.PetId is not null)
                await _petNotifier.ExpenseDeletedAsync(updated.Id);
        }
        catch
        {
            // El notificador ya traga y registra sus propios errores.
        }
    }
}
