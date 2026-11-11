namespace MoneyTracker.Domain.Application.Expense;

public interface IDeleteExpenseSpreadUseCase
{
    /// <summary>
    /// Borra de golpe todas las cuotas del prorrateo al que pertenece el gasto
    /// <paramref name="id"/>, devolviendo su importe al pocket si salieron de uno.
    /// Devuelve nº de cuotas borradas.
    /// </summary>
    Task<int> ExecuteAsync(int id, int userId);
}
