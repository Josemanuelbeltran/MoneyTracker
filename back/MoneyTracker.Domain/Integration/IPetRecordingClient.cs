namespace MoneyTracker.Domain.Integration;

/// <summary>
/// Lo que PetRecording espera recibir de un gasto de mascota. Los tres importes
/// tienen que cuadrar (allí lo valida con un céntimo de margen):
/// OwnerAmount + FamilyAmount = TotalAmount.
/// </summary>
public sealed record PetExpenseSync(
    int MtExpenseId,
    int PetId,
    decimal TotalAmount,
    decimal OwnerAmount,
    decimal FamilyAmount,
    DateOnly Date,
    string Category,
    string? Description);

/// <summary>
/// Puerto hacia PetRecording (otra app del usuario, en la misma máquina).
/// Ninguno de los dos métodos lanza: apuntar un gasto no puede depender de que
/// la otra app esté viva, así que un fallo solo deja rastro en el log.
/// </summary>
public interface IPetRecordingClient
{
    /// <summary>Alta y edición: allí es un upsert por MtExpenseId, no duplica.</summary>
    Task UpsertExpenseAsync(PetExpenseSync expense, CancellationToken cancellationToken = default);

    Task DeleteExpenseAsync(int mtExpenseId, CancellationToken cancellationToken = default);
}
