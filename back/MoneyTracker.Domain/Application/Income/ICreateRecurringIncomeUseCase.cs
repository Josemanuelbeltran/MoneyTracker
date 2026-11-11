using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Income;

public interface ICreateRecurringIncomeUseCase
{
    /// <summary>
    /// Crea un ingreso recurrente: materializa <paramref name="monthsAhead"/> ocurrencias
    /// (una el mismo día de cada mes, empezando por la fecha de la plantilla). Devuelve nº creadas.
    /// </summary>
    Task<int> ExecuteAsync(IncomeEntity template, int monthsAhead = 12);
}
