namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class ExpenseCategoryPatchDto
{
    public string? Name { get; set; }      
    public int? ParentId { get; set; }     

    /// <summary>Pasar la categoría a común (true) o a propia (false). Solo admin; null = no tocar.</summary>
    public bool? IsCommon { get; set; }
}
