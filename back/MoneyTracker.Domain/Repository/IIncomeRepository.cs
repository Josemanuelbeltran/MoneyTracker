using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Repository;

public interface IIncomeRepository
{
    Task<IncomeEntity> CreateAsync(IncomeEntity entity);
    /// <summary>Borra un ingreso del usuario. Si no es suyo, no borra nada.</summary>
    Task<bool> DeleteAsync(int id, int userId);
    /// <summary>Actualiza un ingreso del usuario (la entidad lleva su UserId).</summary>
    Task<IncomeEntity> UpdateAsync(IncomeEntity entity);

    /// <summary>
    /// Actualiza el importe de los ingresos recurrentes de una misma serie
    /// (mismo usuario, cuenta y categoría) con fecha >= fromDate. Devuelve nº de filas.
    /// </summary>
    Task<int> UpdateAmountForwardAsync(int userId, int accountId, int categoryId, DateOnly fromDate, decimal newAmount);

    /// <summary>
    /// Borra los ingresos recurrentes de una misma serie (mismo usuario, cuenta y
    /// categoría) con fecha >= fromDate. Devuelve nº de filas borradas.
    /// </summary>
    Task<int> DeleteSeriesForwardAsync(int userId, int accountId, int categoryId, DateOnly fromDate);
}
