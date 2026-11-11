using MoneyTracker.Domain.Entities;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.Mappers;

public static class ExpenseMapper
{
    public static ExpenseDto ToDto(this ExpenseEntity e)

    {
        return new ExpenseDto
        {
            Id = e.Id,
            UserId = e.UserId,
            AccountId = e.AccountId,
            CategoryId = e.CategoryId,
            Amount = e.Amount,
            ExpenseDate = e.ExpenseDate,
            Description = e.Description,
            IsRecurring = e.IsRecurring,
            PocketId = e.PocketId,
            PetId = e.PetId,
            TotalAmount = e.TotalAmount,
            SplitParts = e.SplitParts,
            SpreadGroup = e.SpreadGroup,
            SpreadIndex = e.SpreadIndex,
            SpreadCount = e.SpreadCount,
            UpdatedAt = DateTime.Now
        };
    }

    public static ExpenseEntity ToDomain(this ExpenseDto d)
    {
        return new ExpenseEntity
        {
            Id = d.Id,
            UserId = d.UserId,
            AccountId = d.AccountId,
            CategoryId = d.CategoryId,
            Amount = d.Amount,
            ExpenseDate = d.ExpenseDate,
            Description = d.Description,
            IsRecurring = d.IsRecurring,
            PocketId = d.PocketId,
            PetId = d.PetId,
            TotalAmount = d.TotalAmount,
            SplitParts = d.SplitParts,
            SpreadGroup = d.SpreadGroup,
            SpreadIndex = d.SpreadIndex,
            SpreadCount = d.SpreadCount,
        };
    }
}