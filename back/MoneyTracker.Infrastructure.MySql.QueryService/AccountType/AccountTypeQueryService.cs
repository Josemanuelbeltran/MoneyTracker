using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities.types;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.DDBBConection;

namespace MoneyTracker.Infrastructure.MySql.QueryService.AccountType;

public class AccountTypeQueryService : IAccountTypeQueryService
{
    private readonly AppDbContext _context;

    public AccountTypeQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AccountTypeEntity>> GetAllAsync()
    {
        var dtos = await _context.AccountTypes
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

        return dtos.Select(d => new AccountTypeEntity { Id = d.Id, Name = d.Name }).ToList();
    }
}
