using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.QueryService.Expense;

public class ExpenseQueryService : IExpenseQueryService
{
    private readonly AppDbContext _context;

    public ExpenseQueryService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<ExpenseEntity?> GetByIdAsync(int id, int userId)
    {
        var dto = await _context.Expenses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        return dto?.ToDomain();
    }

    public async Task<List<ExpenseEntity>> GetByUserAsync(int userId)
    {
        var dtos = await _context.Expenses
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ExpenseDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync();

        return dtos.Select(d => d.ToDomain()).ToList();
    }
}