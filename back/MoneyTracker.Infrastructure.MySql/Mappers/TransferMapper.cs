using MoneyTracker.Domain.Entities;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.Mappers;

public static class TransferMapper
{
    public static TransferDto ToDto(this TransferEntity e)
    {
        return new TransferDto
        {
            Id = e.Id,
            UserId = e.UserId,
            FromAccountId = e.FromAccountId,
            ToAccountId = e.ToAccountId,
            Amount = e.Amount,
            TransferDate = e.TransferDate,
            Description = e.Description,
            UpdatedAt = DateTime.Now
        };
    }

    public static TransferEntity ToDomain(this TransferDto d)
    {
        return new TransferEntity
        {
            Id = d.Id,
            UserId = d.UserId,
            FromAccountId = d.FromAccountId,
            ToAccountId = d.ToAccountId,
            Amount = d.Amount,
            TransferDate = d.TransferDate,
            Description = d.Description
        };
    }

    public static List<TransferEntity> ToDomain(this List<TransferDto> d)
    {
        return d.Select(x => x.ToDomain()).ToList();
    }
}
