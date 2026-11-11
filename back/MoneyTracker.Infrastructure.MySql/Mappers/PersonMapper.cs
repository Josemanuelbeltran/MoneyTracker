using MoneyTracker.Domain.Entities;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.Mappers;

public static class PersonMapper
{
    public static PersonDto ToDto(this PersonEntity e)
    {
        return new PersonDto
        {
            Id = e.Id,
            UserId = e.UserId,
            Name = e.Name,
            UpdatedAt = DateTime.Now
        };
    }

    public static PersonEntity ToDomain(this PersonDto d)
    {
        return new PersonEntity
        {
            Id = d.Id,
            UserId = d.UserId,
            Name = d.Name
        };
    }

    public static List<PersonEntity> ToDomain(this List<PersonDto> d)
    {
        return d.Select(x => x.ToDomain()).ToList();
    }
}
