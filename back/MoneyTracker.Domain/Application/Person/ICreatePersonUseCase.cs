using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.Person;

public interface ICreatePersonUseCase
{
    /// <summary>Crea la persona. Si el usuario ya tiene una con ese nombre, devuelve la que hay.</summary>
    Task<PersonEntity> ExecuteAsync(int userId, string name);
}
