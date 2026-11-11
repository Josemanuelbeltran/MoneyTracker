namespace MoneyTracker.Domain.Exceptions;

/// <summary>
/// El recurso existe y el usuario lo ve, pero no puede hacer esa operación sobre él
/// (p.ej. editar o borrar una categoría común del sistema). Se traduce a HTTP 403.
/// </summary>
public class ForbiddenOperationException : Exception
{
    public ForbiddenOperationException(string message) : base(message) { }
}
