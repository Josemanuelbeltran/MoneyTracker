namespace MoneyTracker.Domain.Application.Income;

public interface IUpdateIncomeSeriesUseCase
{
    /// <summary>
    /// "Editar este y siguientes": cambia el importe del ingreso <paramref name="id"/> y de
    /// todos los de su misma serie recurrente con fecha posterior, sin tocar el histórico.
    /// Devuelve nº de ingresos actualizados.
    /// </summary>
    Task<int> ExecuteAsync(int id, int userId, decimal newAmount);
}
