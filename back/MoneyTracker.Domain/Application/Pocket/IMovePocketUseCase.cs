namespace MoneyTracker.Domain.Application.Pocket;

public interface IMovePocketUseCase
{
    /// <summary>
    /// Mueve dinero entre pockets de una misma cuenta del usuario. Un pocket nulo
    /// representa el saldo "sin asignar" de la cuenta (no hay fila que tocar en ese lado).
    /// </summary>
    Task ExecuteAsync(int? fromPocketId, int? toPocketId, decimal amount, int userId);
}
