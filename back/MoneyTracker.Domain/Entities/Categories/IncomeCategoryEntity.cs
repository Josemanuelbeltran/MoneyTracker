namespace MoneyTracker.Domain.Entities.Categories;
public sealed class IncomeCategoryEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int? ParentId { get; set; }

    /// <summary>Dueño de la categoría. NULL = categoría común del sistema (solo lectura).</summary>
    public int? UserId { get; set; }

    /// <summary>Atajo para el front: las comunes se ven pero no se pueden editar ni borrar.</summary>
    public bool IsCommon => UserId is null;

    public IncomeCategoryEntity() { }

    public IncomeCategoryEntity(string name, int? parentId = null, int? userId = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ParentId = parentId;
        UserId = userId;
    }
}
