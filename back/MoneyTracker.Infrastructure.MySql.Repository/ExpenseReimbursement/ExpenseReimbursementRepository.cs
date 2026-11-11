using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.Repository.ExpenseReimbursement;

public class ExpenseReimbursementRepository : IExpenseReimbursementRepository
{
    private readonly AppDbContext _context;

    public ExpenseReimbursementRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseReimbursementEntity> CreateAsync(ExpenseReimbursementEntity entity)
    {
        var dto = new ExpenseReimbursementDto
        {
            UserId = entity.UserId,
            ExpenseId = entity.ExpenseId,
            Amount = entity.Amount,
            ToAccountId = entity.ToAccountId,
            SettledAt = entity.SettledAt,
            Debtor = entity.Debtor,
            PersonId = entity.PersonId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _context.ExpenseReimbursements.AddAsync(dto);
        await _context.SaveChangesAsync();
        return dto.ToDomain();
    }

    public async Task<bool> UpdateAsync(int id, int userId, decimal? amount, int? toAccountId, int? personId, string? debtor)
    {
        var dto = await _context.ExpenseReimbursements
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (dto is null)
            return false;

        // Solo se pisa lo que venga informado; el resto se queda como estaba.
        if (amount is decimal newAmount) dto.Amount = newAmount;
        if (toAccountId is int newAccount) dto.ToAccountId = newAccount;
        // La persona sí se puede quitar (personId = 0 significa "sin especificar").
        if (personId is int newPerson) dto.PersonId = newPerson > 0 ? newPerson : null;
        if (debtor is not null) dto.Debtor = string.IsNullOrWhiteSpace(debtor) ? null : debtor.Trim();

        dto.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SettleAsync(int id, int userId, DateTime settledAt)
    {
        var rows = await _context.ExpenseReimbursements
            .Where(x => x.Id == id && x.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.SettledAt, settledAt)
                .SetProperty(x => x.UpdatedAt, DateTime.Now));

        return rows > 0;
    }

    public async Task<bool> UnsettleAsync(int id, int userId)
    {
        var rows = await _context.ExpenseReimbursements
            .Where(x => x.Id == id && x.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.SettledAt, (DateTime?)null)
                .SetProperty(x => x.UpdatedAt, DateTime.Now));

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var rows = await _context.ExpenseReimbursements
            .Where(x => x.Id == id && x.UserId == userId)
            .ExecuteDeleteAsync();

        return rows > 0;
    }
}
