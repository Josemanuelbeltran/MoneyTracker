using MoneyTracker.Domain.Entities.Categories;

namespace MoneyTracker.Domain.Repository;

public interface IExpenseCategoryRepository
{
    /// <summary>Crea una categoría. <paramref name="userId"/> null = categoría común.</summary>
    Task<ExpenseCategoryEntity> CreateAsync(int? parentId, string name, int? userId);

    /// <summary>Actualiza nombre, padre y propiedad (UserId) de la categoría.</summary>
    Task<ExpenseCategoryEntity?> UpdateAsync(ExpenseCategoryEntity entity);

    Task<bool> DeleteAsync(int id);
}
