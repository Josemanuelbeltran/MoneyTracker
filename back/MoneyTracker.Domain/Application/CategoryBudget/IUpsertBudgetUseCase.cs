using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.CategoryBudget;

public interface IUpsertBudgetUseCase
{
    /// <summary>Crea o actualiza el presupuesto mensual de una categoría del usuario.</summary>
    Task<CategoryBudgetEntity> ExecuteAsync(int userId, int categoryId, decimal monthlyLimit);
}
