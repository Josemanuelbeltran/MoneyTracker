using MoneyTracker.Domain.Application.Expense;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Exceptions;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.Expense;

public class UpdateSplitSpreadExpenseUseCase : IUpdateSplitSpreadExpenseUseCase
{
    private readonly IExpenseQueryService _expenseQueryService;
    private readonly IExpenseReimbursementQueryService _reimbursementQueryService;
    private readonly IDeleteExpenseUseCase _deleteExpense;
    private readonly ICreateSplitSpreadExpenseUseCase _createSplitSpread;

    public UpdateSplitSpreadExpenseUseCase(
        IExpenseQueryService expenseQueryService,
        IExpenseReimbursementQueryService reimbursementQueryService,
        IDeleteExpenseUseCase deleteExpense,
        ICreateSplitSpreadExpenseUseCase createSplitSpread)
    {
        _expenseQueryService = expenseQueryService;
        _reimbursementQueryService = reimbursementQueryService;
        _deleteExpense = deleteExpense;
        _createSplitSpread = createSplitSpread;
    }

    public async Task<List<ExpenseEntity>> ExecuteAsync(
        int id,
        ExpenseEntity template,
        int? splitParts,
        int spreadMonths = 1,
        decimal? myShareAmount = null,
        int? reimburseToAccountId = null,
        string? debtor = null,
        int? personId = null,
        IReadOnlyList<int?>? debtorPersonIds = null,
        IReadOnlyList<string?>? debtorNames = null)
    {
        var anchor = await _expenseQueryService.GetByIdAsync(id, template.UserId)
                     ?? throw new KeyNotFoundException("Expense no existe");

        // Qué hay que rehacer: si estaba prorrateado, el grupo entero. Cambiar el nº de
        // meses o el reparto afecta a todas las cuotas, no solo a la que se está editando.
        var group = new List<ExpenseEntity> { anchor };
        if (anchor.SpreadGroup is string spreadGroup)
        {
            var all = await _expenseQueryService.GetByUserAsync(template.UserId);
            group = all.Where(e => e.SpreadGroup == spreadGroup).ToList();
        }

        // Se comprueba TODO el grupo antes de borrar nada: si una sola deuda ya está
        // cobrada, no se toca ni una cuota. Ese dinero ya entró en la cuenta de destino
        // y borrar el cobro para recrearlo pendiente descuadraría el saldo.
        foreach (var cuota in group)
        {
            var reimbursements = await _reimbursementQueryService.GetByExpenseIdAsync(cuota.Id, template.UserId);
            if (reimbursements.Any(r => !r.IsPending))
                throw new ForbiddenOperationException(
                    "Este gasto tiene cobros marcados como cobrados. Desmárcalos en Cobros antes de cambiar el reparto.");
        }

        // Rehacer = borrar y volver a crear. El borrado unitario ya devuelve al sobre lo
        // que salió (tu parte más lo que adelantaste) y avisa a PetRecording, y la FK se
        // lleva por delante los reembolsos de cada cuota.
        foreach (var cuota in group)
            await _deleteExpense.ExecuteAsync(cuota.Id, template.UserId);

        // La plantilla se crea de cero: el id viejo ya no existe.
        template.Id = 0;

        return await _createSplitSpread.ExecuteAsync(
            template, splitParts, spreadMonths, myShareAmount, reimburseToAccountId, debtor, personId,
            debtorPersonIds, debtorNames);
    }
}
