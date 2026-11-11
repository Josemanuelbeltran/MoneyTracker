using MoneyTracker.Domain.Entities;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.Mappers;

public static class AccountMapper
{
    public static AccountDto ToDto(this AccountEntity e) 
    {
        return new AccountDto{
            Id = e.Id,
            UserId = e.UserId,
            AccountTypeId = e.AccountTypeId,
            Name = e.Name,
            Balance = e.Balance,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        
        };
    }

    public static AccountEntity ToDomain(this AccountDto d)
    {
        return new AccountEntity
        {
            Id = d.Id,
            UserId = d.UserId,
            AccountTypeId = d.AccountTypeId,
            Name = d.Name,
            Balance = d.Balance
        };
    }
    public static List<AccountEntity> ToDomain(this List<AccountDto>d)
    {
        return d.Select(d => d.ToDomain()).ToList();
    }
}