using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.Repository.CategoryBudget;

public class CategoryBudgetRepository : ICategoryBudgetRepository
{
    private readonly AppDbContext _context;

    public CategoryBudgetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryBudgetEntity> UpsertAsync(int userId, int categoryId, decimal monthlyLimit)
    {
        var budget = await _context.CategoryBudgets
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CategoryId == categoryId);

        if (budget is null)
        {
            budget = new CategoryBudgetDto
            {
                UserId = userId,
                CategoryId = categoryId,
                MonthlyLimit = monthlyLimit,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await _context.CategoryBudgets.AddAsync(budget);
        }
        else
        {
            budget.MonthlyLimit = monthlyLimit;
            budget.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return budget.ToDomain();
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var rows = await _context.CategoryBudgets
            .Where(x => x.Id == id && x.UserId == userId)
            .ExecuteDeleteAsync();

        return rows > 0;
    }
}
