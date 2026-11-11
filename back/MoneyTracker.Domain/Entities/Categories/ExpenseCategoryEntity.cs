using System.Text.Json.Serialization;

namespace MoneyTracker.Domain.Entities.Categories;

public class ExpenseCategoryEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }

    /// <summary>Dueño de la categoría. NULL = categoría común del sistema (solo lectura).</summary>
    public int? UserId { get; set; }

    /// <summary>Atajo para el front: las comunes se ven pero no se pueden editar ni borrar.</summary>
    public bool IsCommon => UserId is null;

    public List<ExpenseCategoryEntity> Children { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [JsonIgnore]                
    public ExpenseCategoryEntity? Parent { get; set; }
}
