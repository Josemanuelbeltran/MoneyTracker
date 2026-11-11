namespace MoneyTracker.Domain.Entities;

/// <summary>Presupuesto mensual (recurrente) para una categoría o subcategoría de gasto.</summary>
public sealed class CategoryBudgetEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public decimal MonthlyLimit { get; set; }

    public CategoryBudgetEntity() { }

    public CategoryBudgetEntity(int userId, int categoryId, decimal monthlyLimit)
    {
        UserId = userId;
        CategoryId = categoryId;
        MonthlyLimit = monthlyLimit;
    }
}
