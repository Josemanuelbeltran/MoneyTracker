using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.QueryService.Pocket;

public class PocketQueryService : IPocketQueryService
{
    private readonly AppDbContext _context;

    public PocketQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PocketEntity>> GetByUserAsync(int userId)
    {
        var dtos = await _context.Pockets
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.AccountId)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return dtos.Select(d => d.ToDomain()).ToList();
    }
}
