using MoneyTracker.Domain.Entities;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.Mappers;

public static class IncomeMapper
{
    public static IncomeDto ToDto(this IncomeEntity e)
    {
        return new IncomeDto
        {
            Id = e.Id,
            UserId = e.UserId,
            AccountId = e.AccountId,
            CategoryId = e.CategoryId,
            Amount = e.Amount,
            IncomeDate = e.IncomeDate,
            Description = e.Description,
            IsRecurring = e.IsRecurring,
            UpdatedAt = DateTime.Now
        };
    }

    public static IncomeEntity ToDomain(this IncomeDto d)
    {
        return new IncomeEntity
        {
            Id = d.Id,
            UserId = d.UserId,
            AccountId = d.AccountId,
            CategoryId = d.CategoryId,
            Amount = d.Amount,
            IncomeDate = d.IncomeDate,
            Description = d.Description,
            IsRecurring = d.IsRecurring,
        };
    }
}
