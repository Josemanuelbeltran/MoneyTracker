namespace MoneyTracker.Domain.Application.Recurring;

public interface IGenerateRecurringUseCase
{
    /// <summary>
    /// Genera, para el mes actual, los movimientos fijos (ingresos y gastos marcados como
    /// recurrentes) que aún no tengan una entrada este mes. Devuelve cuántos creó.
    /// </summary>
    Task<int> ExecuteAsync(int userId);
}
