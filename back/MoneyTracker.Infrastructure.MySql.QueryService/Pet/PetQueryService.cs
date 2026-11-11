using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.QueryService.Pet;

public class PetQueryService : IPetQueryService
{
    private readonly AppDbContext _context;

    public PetQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PetEntity?> GetByIdAsync(int id, int userId)
    {
        var row = await _context.Pets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        return row?.ToDomain();
    }

    public async Task<List<PetEntity>> GetByUserAsync(int userId)
    {
        var rows = await _context.Pets
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ToListAsync();

        return rows.ToDomain();
    }
}
