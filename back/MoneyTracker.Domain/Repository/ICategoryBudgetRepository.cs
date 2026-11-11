using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Repository;

public interface ICategoryBudgetRepository
{
    /// <summary>Crea o actualiza (por user+categoría) el presupuesto mensual.</summary>
    Task<CategoryBudgetEntity> UpsertAsync(int userId, int categoryId, decimal monthlyLimit);

    /// <summary>Borra un presupuesto del usuario. Si no es suyo, no borra nada.</summary>
    Task<bool> DeleteAsync(int id, int userId);
}
