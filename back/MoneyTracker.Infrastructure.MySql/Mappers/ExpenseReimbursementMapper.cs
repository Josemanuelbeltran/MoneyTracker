using MoneyTracker.Domain.Entities;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.Mappers;

public static class ExpenseReimbursementMapper
{
    public static ExpenseReimbursementDto ToDto(this ExpenseReimbursementEntity e)
    {
        return new ExpenseReimbursementDto
        {
            Id = e.Id,
            UserId = e.UserId,
            ExpenseId = e.ExpenseId,
            Amount = e.Amount,
            ToAccountId = e.ToAccountId,
            SettledAt = e.SettledAt,
            Debtor = e.Debtor,
            PersonId = e.PersonId,
            UpdatedAt = DateTime.Now
        };
    }

    public static ExpenseReimbursementEntity ToDomain(this ExpenseReimbursementDto d)
    {
        return new ExpenseReimbursementEntity
        {
            Id = d.Id,
            UserId = d.UserId,
            ExpenseId = d.ExpenseId,
            Amount = d.Amount,
            ToAccountId = d.ToAccountId,
            SettledAt = d.SettledAt,
            Debtor = d.Debtor,
            PersonId = d.PersonId
        };
    }

    public static List<ExpenseReimbursementEntity> ToDomain(this List<ExpenseReimbursementDto> d)
    {
        return d.Select(x => x.ToDomain()).ToList();
    }
}
