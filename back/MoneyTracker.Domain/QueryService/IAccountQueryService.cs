using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.QueryService;

public interface IAccountQueryService
{
    /// <summary>Cuenta del usuario. Devuelve null si no existe o no es suya.</summary>
    Task<AccountEntity?> GetByIdAsync(int id, int userId);

    Task<List<AccountEntity>> GetAllByUserIdAsync(int userId);
}
