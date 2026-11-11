using MoneyTracker.Domain.Entities;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(this UserEntity e)
    {
        return new UserDto
        {
            Id = e.Id,
            Name = e.Name,
            Email = e.Email,
            PasswordHash = e.PasswordHash,
            PhoneNumber = e.PhoneNumber,
            VerifiedPhone = e.VerifiedPhone,
            IsAdmin = e.IsAdmin,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

    }

    public static UserEntity ToDomain(this UserDto d)
    {
        return new UserEntity
        {
            Id = d.Id,
            Name = d.Name,
            Email = d.Email,
            PasswordHash = d.PasswordHash,
            PhoneNumber = d.PhoneNumber,
            VerifiedPhone = d.VerifiedPhone,
            IsAdmin = d.IsAdmin,
        };
    }
}