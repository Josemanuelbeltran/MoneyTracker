using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities.Categories;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.QueryService.IncomeCategory;

public class IncomeCategoryQueryService : IIncomeCategoryQueryService
{
    private readonly AppDbContext _context;

    public IncomeCategoryQueryService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Visibles para el usuario: las comunes (user_id NULL) y las suyas.</summary>
    private IQueryable<IncomeCategoryDto> Visible(int userId) =>
        _context.IncomeCategories
            .AsNoTracking()
            .Where(x => x.UserId == null || x.UserId == userId);

    public async Task<List<IncomeCategoryEntity>> GetAllAsync(int userId)
    {
        var dtos = await Visible(userId)
            .OrderBy(x => x.Name)
            .ToListAsync();

        return dtos.Select(d => d.ToDomain()).ToList();
    }

    public async Task<IncomeCategoryEntity?> GetByIdAsync(int id, int userId)
    {
        var dto = await Visible(userId).FirstOrDefaultAsync(x => x.Id == id);
        return dto?.ToDomain();
    }
}
