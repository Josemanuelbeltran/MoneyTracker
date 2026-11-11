using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.QueryService.Account;

public class AccountQueryService : IAccountQueryService
{
    private readonly AppDbContext  _context;

    public AccountQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AccountEntity?> GetByIdAsync(int id, int userId)
    {
        var dto = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        return dto?.ToDomain();
    }

    public async Task<List<AccountEntity>> GetAllByUserIdAsync(int userId)
    {
        var dtos = await _context.Accounts
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync();

        return dtos.ToDomain();
    }

}