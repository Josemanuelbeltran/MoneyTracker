using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.QueryService.Transfer;

public class TransferQueryService : ITransferQueryService
{
    private readonly AppDbContext _context;

    public TransferQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransferEntity>> GetByUserAsync(int userId)
    {
        var dtos = await _context.Transfers
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.TransferDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync();

        return dtos.Select(d => d.ToDomain()).ToList();
    }
}
