using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.QueryService.Person;

public class PersonQueryService : IPersonQueryService
{
    private readonly AppDbContext _context;

    public PersonQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PersonEntity>> GetByUserAsync(int userId)
    {
        var rows = await _context.People
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ToListAsync();

        return rows.ToDomain();
    }

    public Task<bool> ExistsAsync(int id, int userId)
    {
        return _context.People.AnyAsync(x => x.Id == id && x.UserId == userId);
    }
}
