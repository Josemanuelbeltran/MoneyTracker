using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.Repository.Income;

public class IncomeRepository : IIncomeRepository
{
    private readonly AppDbContext _context;

    public IncomeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IncomeEntity> CreateAsync(IncomeEntity entity)
    {
        var income = new IncomeDto
        {
            UserId      = entity.UserId,
            AccountId   = entity.AccountId,
            CategoryId  = entity.CategoryId,
            Amount      = entity.Amount,
            IncomeDate  = entity.IncomeDate,
            Description = entity.Description,
            IsRecurring = entity.IsRecurring,
            CreatedAt   = DateTime.Now,
            UpdatedAt   = DateTime.Now
        };

        await _context.Incomes.AddAsync(income);
        await _context.SaveChangesAsync();
        return income.ToDomain();
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var rows = await _context.Incomes
            .Where(x => x.Id == id && x.UserId == userId)
            .ExecuteDeleteAsync();

        return rows > 0;
    }

    public async Task<IncomeEntity> UpdateAsync(IncomeEntity entity)
    {
        var income = await _context.Incomes
            .FirstOrDefaultAsync(x => x.Id == entity.Id && x.UserId == entity.UserId)
            ?? throw new KeyNotFoundException("Income no existe");

        income.AccountId   = entity.AccountId;
        income.CategoryId  = entity.CategoryId;
        income.Amount      = entity.Amount;
        income.IncomeDate  = entity.IncomeDate;
        income.Description = entity.Description;
        income.IsRecurring = entity.IsRecurring;
        income.UpdatedAt   = DateTime.Now;

        await _context.SaveChangesAsync();
        return income.ToDomain();
    }

    public async Task<int> UpdateAmountForwardAsync(int userId, int accountId, int categoryId, DateOnly fromDate, decimal newAmount)
    {
        return await _context.Incomes
            .Where(x => x.UserId == userId
                     && x.AccountId == accountId
                     && x.CategoryId == categoryId
                     && x.IsRecurring
                     && x.IncomeDate >= fromDate)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Amount, newAmount)
                .SetProperty(x => x.UpdatedAt, DateTime.Now));
    }

    public async Task<int> DeleteSeriesForwardAsync(int userId, int accountId, int categoryId, DateOnly fromDate)
    {
        return await _context.Incomes
            .Where(x => x.UserId == userId
                     && x.AccountId == accountId
                     && x.CategoryId == categoryId
                     && x.IsRecurring
                     && x.IncomeDate >= fromDate)
            .ExecuteDeleteAsync();
    }
}
