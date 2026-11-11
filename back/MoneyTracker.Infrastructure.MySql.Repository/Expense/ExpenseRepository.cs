using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.Repository.Expense;

public class ExpenseRepository : IExpenseRepository
{
    private readonly AppDbContext _context;

    public ExpenseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseEntity> CreateAsync(ExpenseEntity entity)
    {
        var expense = new ExpenseDto
        {
            UserId      = entity.UserId,
            AccountId   = entity.AccountId,
            CategoryId  = entity.CategoryId,
            Amount      = entity.Amount,
            ExpenseDate = entity.ExpenseDate,
            Description = entity.Description,
            IsRecurring = entity.IsRecurring,
            PocketId = entity.PocketId,
            // Mascota: el gasto se manda a PetRecording con este id.
            PetId = entity.PetId,
            // Gasto parcial: amount ya es la parte del usuario, esto es el contexto.
            TotalAmount = entity.TotalAmount,
            SplitParts  = entity.SplitParts,
            // Prorrateo: identifican la cuota dentro del grupo de N.
            SpreadGroup = entity.SpreadGroup,
            SpreadIndex = entity.SpreadIndex,
            SpreadCount = entity.SpreadCount,
            UpdatedAt = DateTime.Now

        };
    
        await _context.Expenses.AddAsync(expense);
        await _context.SaveChangesAsync();
        return expense.ToDomain();
    }


    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var rows = await _context.Expenses
            .Where(x => x.Id == id && x.UserId == userId)
            .ExecuteDeleteAsync();

        return rows > 0; 
    }

    public async Task<ExpenseEntity> UpdateAsync(ExpenseEntity entity)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(x => x.Id == entity.Id && x.UserId == entity.UserId)
            ?? throw new KeyNotFoundException("Expense no existe");

        expense.AccountId   = entity.AccountId;
        expense.CategoryId  = entity.CategoryId;
        expense.Amount      = entity.Amount;
        expense.ExpenseDate = entity.ExpenseDate;
        expense.Description = entity.Description;
        expense.IsRecurring = entity.IsRecurring;
        expense.PocketId    = entity.PocketId;
        expense.PetId       = entity.PetId;
        expense.UpdatedAt   = DateTime.Now;

        await _context.SaveChangesAsync();
        return expense.ToDomain();
    }
}