using MoneyTracker.Domain.Entities;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.Mappers;

public static class PetMapper
{
    public static PetDto ToDto(this PetEntity e)
    {
        return new PetDto
        {
            Id = e.Id,
            UserId = e.UserId,
            Name = e.Name
        };
    }

    public static PetEntity ToDomain(this PetDto d)
    {
        return new PetEntity
        {
            Id = d.Id,
            UserId = d.UserId,
            Name = d.Name
        };
    }

    public static List<PetEntity> ToDomain(this List<PetDto> d)
    {
        return d.Select(x => x.ToDomain()).ToList();
    }
}
