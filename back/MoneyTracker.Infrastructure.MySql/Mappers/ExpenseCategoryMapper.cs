using MoneyTracker.Domain.Entities.Categories;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.Mappers;

public static class ExpenseCategoryMapper
{
        public static ExpenseCategoryEntity ToDomain(this ExpenseCategoryDto d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            ParentId = d.ParentId,
            UserId = d.UserId,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt,
            Children = new()
        };

        public static ExpenseCategoryDto ToDto(this ExpenseCategoryEntity e) => new()
        {
            Id = e.Id,
            Name = e.Name,
            ParentId = e.ParentId,
            UserId = e.UserId,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };

}
