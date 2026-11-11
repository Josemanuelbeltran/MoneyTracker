using MoneyTracker.Domain.Entities.Categories;

namespace MoneyTracker.Domain.Repository;

public interface IIncomeCategoryRepository
{
    /// <summary>Crea una categoría. <paramref name="userId"/> null = categoría común.</summary>
    Task<IncomeCategoryEntity> CreateAsync(int? parentId, string name, int? userId);

    /// <summary>Actualiza nombre y propiedad (UserId) de la categoría.</summary>
    Task<IncomeCategoryEntity?> UpdateAsync(IncomeCategoryEntity entity);

    Task<bool> DeleteAsync(int id);
}
