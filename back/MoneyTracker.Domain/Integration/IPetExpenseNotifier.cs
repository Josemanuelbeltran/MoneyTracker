using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Integration;

/// <summary>
/// Avisa a PetRecording de los gastos de mascota. Traduce el modelo de
/// MoneyTracker (donde el importe del gasto es SIEMPRE tu parte) al que espera
/// la otra app (total, lo tuyo y lo de la familia).
///
/// Como el cliente que hay debajo, nunca lanza.
/// </summary>
public interface IPetExpenseNotifier
{
    /// <summary>Alta o edición. Si el gasto no es de mascota (PetId null), no hace nada.</summary>
    Task ExpenseSavedAsync(ExpenseEntity expense, CancellationToken cancellationToken = default);

    Task ExpenseDeletedAsync(int expenseId, CancellationToken cancellationToken = default);
}
