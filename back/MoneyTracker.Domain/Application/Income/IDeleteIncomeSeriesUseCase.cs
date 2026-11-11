namespace MoneyTracker.Domain.Application.Income;

public interface IDeleteIncomeSeriesUseCase
{
    /// <summary>
    /// "Borrar este y siguientes": elimina el ingreso <paramref name="id"/> y todos los de su
    /// misma serie recurrente con fecha posterior, sin tocar el histórico.
    /// Devuelve nº de ingresos borrados.
    /// </summary>
    Task<int> ExecuteAsync(int id, int userId);
}
