using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities.types;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.Repository.AccountType;

public class AccountTypeRepository : IAccountTypeRepository
{
    private readonly AppDbContext _context;

    public AccountTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AccountTypeEntity> CreateAsync(string name)
    {
        var type = new AccountTypeDto
        {
            Name = name
        };
        
        await _context.AccountTypes.AddAsync(type);
        await _context.SaveChangesAsync();
        return type.ToDomain();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var rows = await _context.AccountTypes
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync();

        return rows > 0; 
    }
    
    public async Task<bool> UpdateAsync(int id, string name)
    {
        var type = await _context.AccountTypes
            .FirstOrDefaultAsync(x => x.Id == id);

        if (type == null)
            return false;

        type.Name = name;   

        await _context.SaveChangesAsync();
        return true;
    }

}