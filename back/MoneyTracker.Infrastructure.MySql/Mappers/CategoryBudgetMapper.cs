using MoneyTracker.Domain.Entities;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.Mappers;

public static class CategoryBudgetMapper
{
    public static CategoryBudgetEntity ToDomain(this CategoryBudgetDto d)
    {
        return new CategoryBudgetEntity
        {
            Id = d.Id,
            UserId = d.UserId,
            CategoryId = d.CategoryId,
            MonthlyLimit = d.MonthlyLimit
        };
    }

    public static List<CategoryBudgetEntity> ToDomain(this List<CategoryBudgetDto> d)
    {
        return d.Select(x => x.ToDomain()).ToList();
    }
}
