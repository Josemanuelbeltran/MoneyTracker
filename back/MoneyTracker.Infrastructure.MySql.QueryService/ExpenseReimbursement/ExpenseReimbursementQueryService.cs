using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.QueryService.ExpenseReimbursement;

public class ExpenseReimbursementQueryService : IExpenseReimbursementQueryService
{
    private readonly AppDbContext _context;

    public ExpenseReimbursementQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseReimbursementEntity?> GetByIdAsync(int id, int userId)
    {
        var dto = await _context.ExpenseReimbursements
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        return dto?.ToDomain();
    }

    public async Task<List<ExpenseReimbursementEntity>> GetByExpenseIdAsync(int expenseId, int userId)
    {
        var rows = await _context.ExpenseReimbursements
            .AsNoTracking()
            .Where(x => x.ExpenseId == expenseId && x.UserId == userId)
            .OrderBy(x => x.Id)
            .ToListAsync();

        return rows.ToDomain();
    }

    public async Task<List<ExpenseReimbursementEntity>> GetByUserAsync(int userId)
    {
        var rows = await _context.ExpenseReimbursements
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            // Primero lo que todavia te deben, y dentro de eso lo mas reciente.
            .OrderBy(x => x.SettledAt == null ? 0 : 1)
            .ThenByDescending(x => x.Id)
            .ToListAsync();

        return rows.ToDomain();
    }
}
