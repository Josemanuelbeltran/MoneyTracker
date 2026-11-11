using MoneyTracker.Domain.Entities.Categories;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.Mappers;

public static class IncomeCategoryMapper
{
    public static IncomeCategoryDto ToDto(this IncomeCategoryEntity e) 
    {
        return new IncomeCategoryDto
        {
            Id = e.Id,
            Name = e.Name,
            ParentId = e.ParentId,
            UserId = e.UserId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
    }

    public static IncomeCategoryEntity ToDomain(this IncomeCategoryDto d)
    {
        return new IncomeCategoryEntity
        {
            Id = d.Id,
            Name = d.Name,
            ParentId = d.ParentId,
            UserId = d.UserId
        };
    }
}
