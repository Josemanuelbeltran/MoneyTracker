using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Expense;

public interface IUpdateSplitSpreadExpenseUseCase
{
    /// <summary>
    /// Rehace el reparto de un gasto ya guardado: cambiar entre cuántas partes se
    /// divide, quién te debe cada una, si lo adelantaste tú o en cuántos meses se
    /// prorratea. Los parámetros son los mismos que al crearlo
    /// (<see cref="ICreateSplitSpreadExpenseUseCase"/>) y el <c>Amount</c> de la
    /// plantilla vuelve a ser el importe TOTAL.
    ///
    /// Si el gasto está prorrateado se rehace el grupo entero, no solo la cuota que
    /// se está editando: cambiar el número de meses o el reparto afecta a todas.
    ///
    /// Rehacer es borrar y volver a crear, así que las cuotas resultantes tienen
    /// ids nuevos. A cambio no hay que mantener dos caminos para la misma cuenta y
    /// el dinero vuelve a los sobres por el mismo sitio que en un borrado normal.
    ///
    /// Lanza <see cref="Exceptions.ForbiddenOperationException"/> si alguna de las
    /// deudas del gasto ya está marcada como cobrada: ese dinero ya entró en la
    /// cuenta de destino y borrar el cobro para recrearlo descuadraría el saldo.
    /// Devuelve las cuotas resultantes.
    /// </summary>
    Task<List<ExpenseEntity>> ExecuteAsync(
        int id,
        ExpenseEntity template,
        int? splitParts,
        int spreadMonths = 1,
        decimal? myShareAmount = null,
        int? reimburseToAccountId = null,
        string? debtor = null,
        int? personId = null,
        IReadOnlyList<int?>? debtorPersonIds = null,
        IReadOnlyList<string?>? debtorNames = null);
}
