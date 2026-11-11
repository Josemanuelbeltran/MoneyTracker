using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.Repository.Account;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AccountEntity?> CreateAsync(AccountEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var name = (entity.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la cuenta es obligatorio.", nameof(entity.Name));
        
        var dto = new AccountDto
        {
            UserId        = entity.UserId,
            AccountTypeId = entity.AccountTypeId,
            Name          = name,
            Balance       = entity.Balance
        };

        _context.Accounts.Add(dto);
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("foreign key constraint fails", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new InvalidOperationException("El usuario o el tipo de cuenta no existen.", ex);
        }
        await _context.Entry(dto).ReloadAsync();
        
        return dto.ToDomain();
    }

    public async Task<bool?> DeleteAsync(int id, int userId)
    {
        
        var rows = await _context.Accounts
            .Where(x => x.Id == id && x.UserId == userId)
            .ExecuteDeleteAsync();
        
        return rows > 0; 
    }

    public async Task<AccountEntity?> UpdateAsync(AccountEntity entity)
    {
        var dto = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == entity.Id && a.UserId == entity.UserId);
        if (dto is null) return null;

        var entry = _context.Entry(dto);

        // NAME
        if (!string.IsNullOrWhiteSpace(entity.Name) && entity.Name != dto.Name)
        {
            dto.Name = entity.Name;
            entry.Property(x => x.Name).IsModified = true;
        }

        // ACCOUNT TYPE
        if (entity.AccountTypeId != dto.AccountTypeId && entity.AccountTypeId > 0)
        {
            bool exists = await _context.AccountTypes.AnyAsync(t => t.Id == entity.AccountTypeId);
            if (!exists)
                throw new InvalidOperationException($"AccountTypeId {entity.AccountTypeId} no existe.");

            dto.AccountTypeId = entity.AccountTypeId;
            entry.Property(x => x.AccountTypeId).IsModified = true;
        }

        // BALANCE
        if (entity.Balance != dto.Balance)
        {
            dto.Balance = entity.Balance;
            entry.Property(x => x.Balance).IsModified = true;
        }

        // Protegidas
        entry.Property(x => x.CreatedAt).IsModified = false;
        entry.Property(x => x.UserId).IsModified    = false;
        entry.Property(x => x.UpdatedAt).IsModified = false;

        await _context.SaveChangesAsync();
        return dto.ToDomain();
    }


}