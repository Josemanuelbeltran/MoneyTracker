using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.QueryService.CategoryBudget;

public class CategoryBudgetQueryService : ICategoryBudgetQueryService
{
    private readonly AppDbContext _context;

    public CategoryBudgetQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryBudgetEntity>> GetByUserAsync(int userId)
    {
        var dtos = await _context.CategoryBudgets
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync();

        return dtos.Select(d => d.ToDomain()).ToList();
    }
}
