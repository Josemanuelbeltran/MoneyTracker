using MoneyTracker.Domain.Entities.types;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.Mappers;

public static class AccountTypeMapper
{
    public static AccountTypeDto ToDto(this AccountTypeEntity e)
    {
        return new AccountTypeDto
        {
            Id = e.Id,
            Name = e.Name
        };
    }

    public static AccountTypeEntity ToDomain(this AccountTypeDto d)
    {
        return new AccountTypeEntity
        {
            Id = d.Id,
            Name = d.Name
        };
    }
}