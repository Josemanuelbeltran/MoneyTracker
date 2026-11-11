using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities.Categories;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.Repository.ExpenseCategory;

public class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly AppDbContext _context;

    public ExpenseCategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseCategoryEntity> CreateAsync(int? parentId, string name, int? userId)
    {
        var category = new ExpenseCategoryDto
        {
            ParentId = parentId,
            Name = name,
            UserId = userId,          // null => categoría común
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ExpenseCategories.Add(category);
        await _context.SaveChangesAsync();

        return category.ToDomain();
    }

    public async Task<ExpenseCategoryEntity?> UpdateAsync(ExpenseCategoryEntity entity)
    {
        // Adjuntamos el entity con estado Modified solo en los campos que cambian
        var tracked = await _context.ExpenseCategories.FirstOrDefaultAsync(x => x.Id == entity.Id);
        if (tracked is null) return null;

        // Aplicar cambios mínimos
        if (tracked.Name != entity.Name)         tracked.Name = entity.Name;
        if (tracked.ParentId != entity.ParentId) tracked.ParentId = entity.ParentId;
        if (tracked.UserId != entity.UserId)     tracked.UserId = entity.UserId;

        await _context.SaveChangesAsync();
        return tracked.ToDomain();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var rows = await _context.ExpenseCategories
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync();

        return rows > 0;
    }
}
