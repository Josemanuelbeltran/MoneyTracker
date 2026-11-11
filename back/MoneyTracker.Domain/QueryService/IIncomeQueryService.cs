using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.QueryService;

public interface IIncomeQueryService
{
    /// <summary>Ingreso del usuario. Devuelve null si no existe o no es suyo.</summary>
    Task<IncomeEntity?> GetByIdAsync(int id, int userId);

    Task<List<IncomeEntity>> GetByUserAsync(int userId);
}
