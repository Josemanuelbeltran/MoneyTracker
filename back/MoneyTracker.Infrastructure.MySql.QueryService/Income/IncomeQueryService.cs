using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.QueryService.Income;

public class IncomeQueryService : IIncomeQueryService
{
    private readonly AppDbContext _context;

    public IncomeQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IncomeEntity?> GetByIdAsync(int id, int userId)
    {
        var dto = await _context.Incomes
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        return dto?.ToDomain();
    }

    public async Task<List<IncomeEntity>> GetByUserAsync(int userId)
    {
        var dtos = await _context.Incomes
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IncomeDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync();

        return dtos.Select(d => d.ToDomain()).ToList();
    }
}
