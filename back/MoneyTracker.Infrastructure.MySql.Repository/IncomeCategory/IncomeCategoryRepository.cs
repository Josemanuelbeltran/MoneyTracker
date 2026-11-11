using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities.Categories;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.Repository.IncomeCategory;

public class IncomeCategoryRepository : IIncomeCategoryRepository
{
    private readonly AppDbContext _context;

    public IncomeCategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IncomeCategoryEntity> CreateAsync(int? parentId, string name, int? userId)
    {
        var category = new IncomeCategoryDto
        {
            ParentId = parentId,
            Name = name,
            UserId = userId,          // null => categoría común
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.IncomeCategories.Add(category);
        await _context.SaveChangesAsync();

        return category.ToDomain();
    }

    public async Task<IncomeCategoryEntity?> UpdateAsync(IncomeCategoryEntity entity)
    {
        var dto = await _context.IncomeCategories.FirstOrDefaultAsync(x => x.Id == entity.Id);
        if (dto is null)
            return null;

        dto.Name = entity.Name;
        dto.ParentId = entity.ParentId;
        dto.UserId = entity.UserId;
        dto.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return dto.ToDomain();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var rows = await _context.IncomeCategories
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync();

        return rows > 0;
    }
}
