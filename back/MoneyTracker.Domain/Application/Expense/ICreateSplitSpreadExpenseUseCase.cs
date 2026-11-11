using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Expense;

public interface ICreateSplitSpreadExpenseUseCase
{
    /// <summary>
    /// Crea un gasto compartido y/o prorrateado. El <c>Amount</c> de la plantilla es
    /// siempre el importe TOTAL tecleado por el usuario; primero se calcula su parte
    /// y luego se prorratea en <paramref name="spreadMonths"/> cuotas mensuales.
    ///
    /// Su parte se puede expresar de dos formas, excluyentes entre sí:
    /// <list type="bullet">
    /// <item><paramref name="splitParts"/>: reparto a partes iguales, paga 1 de N.</item>
    /// <item><paramref name="myShareAmount"/>: reparto libre, su parte en euros (un 70/30).</item>
    /// </list>
    ///
    /// Si además adelantó el dinero de los demás, <paramref name="reimburseToAccountId"/>
    /// indica en qué cuenta le devolverán el resto: sale el total de la cuenta con la que
    /// pagó y queda un reembolso pendiente hasta que lo marque como cobrado.
    /// Devuelve las cuotas creadas.
    /// </summary>
    Task<List<ExpenseEntity>> ExecuteAsync(
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
