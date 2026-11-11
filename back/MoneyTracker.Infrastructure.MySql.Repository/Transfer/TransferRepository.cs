using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.Repository.Transfer;

public class TransferRepository : ITransferRepository
{
    private readonly AppDbContext _context;

    public TransferRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TransferEntity> CreateAsync(TransferEntity entity)
    {
        var transfer = new TransferDto
        {
            UserId = entity.UserId,
            FromAccountId = entity.FromAccountId,
            ToAccountId = entity.ToAccountId,
            Amount = entity.Amount,
            TransferDate = entity.TransferDate,
            Description = entity.Description,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _context.Transfers.AddAsync(transfer);
        await _context.SaveChangesAsync();
        return transfer.ToDomain();
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var rows = await _context.Transfers
            .Where(x => x.Id == id && x.UserId == userId)
            .ExecuteDeleteAsync();

        return rows > 0;
    }
}
