using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;


namespace MoneyTracker.Infrastructure.MySql.Repository.User;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity> CreateAsync(string name,string email, string passwordHash, string? phoneNumber, bool? verifiedPhone)
    {
        var dto = new UserDto
        {
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            PhoneNumber = phoneNumber,
            VerifiedPhone = verifiedPhone ?? false
        };

        await _context.Users.AddAsync(dto);
        await _context.SaveChangesAsync();

        return dto.ToDomain();
    }
}