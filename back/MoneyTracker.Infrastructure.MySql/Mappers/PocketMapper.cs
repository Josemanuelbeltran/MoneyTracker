using MoneyTracker.Domain.Entities;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.Mappers;

public static class PocketMapper
{
    public static PocketDto ToDto(this PocketEntity e)
    {
        return new PocketDto
        {
            Id = e.Id,
            UserId = e.UserId,
            AccountId = e.AccountId,
            Name = e.Name,
            Balance = e.Balance,
            UpdatedAt = DateTime.Now
        };
    }

    public static PocketEntity ToDomain(this PocketDto d)
    {
        return new PocketEntity
        {
            Id = d.Id,
            UserId = d.UserId,
            AccountId = d.AccountId,
            Name = d.Name,
            Balance = d.Balance
        };
    }

    public static List<PocketEntity> ToDomain(this List<PocketDto> d)
    {
        return d.Select(x => x.ToDomain()).ToList();
    }
}
