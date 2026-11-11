using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.Repository.Pocket;

public class PocketRepository : IPocketRepository
{
    private readonly AppDbContext _context;

    public PocketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PocketEntity> CreateAsync(PocketEntity entity)
    {
        var pocket = new PocketDto
        {
            UserId = entity.UserId,
            AccountId = entity.AccountId,
            Name = entity.Name,
            Balance = entity.Balance,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _context.Pockets.AddAsync(pocket);
        await _context.SaveChangesAsync();
        return pocket.ToDomain();
    }

    public async Task<PocketEntity?> UpdateAsync(int id, string name, int userId)
    {
        var pocket = await _context.Pockets.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (pocket is null) return null;

        pocket.Name = name;
        pocket.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return pocket.ToDomain();
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var rows = await _context.Pockets
            .Where(x => x.Id == id && x.UserId == userId)
            .ExecuteDeleteAsync();

        return rows > 0;
    }

    public async Task MoveAsync(int? fromPocketId, int? toPocketId, decimal amount, int userId)
    {
        // Un pocket nulo = saldo "sin asignar" de la cuenta (no hay fila que tocar).
        if (fromPocketId is int fromId)
        {
            var from = await _context.Pockets.FirstOrDefaultAsync(x => x.Id == fromId && x.UserId == userId)
                       ?? throw new KeyNotFoundException("El pocket de origen no existe");
            // A propósito no se comprueba el saldo: un pocket puede quedarse en negativo.
            // Es más fiel a la realidad (el gasto ya lo has pagado) y luego lo cuadras
            // con una transferencia. Bloquearlo obligaba a apuntar el gasto donde no era.
            from.Balance -= amount;
            from.UpdatedAt = DateTime.Now;
        }

        if (toPocketId is int toId)
        {
            var to = await _context.Pockets.FirstOrDefaultAsync(x => x.Id == toId && x.UserId == userId)
                     ?? throw new KeyNotFoundException("El pocket de destino no existe");
            to.Balance += amount;
            to.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
    }
}
