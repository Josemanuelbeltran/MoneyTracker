using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.QueryService.User;

public class UserQueryService : IUserQueryService
{
    private readonly AppDbContext  _context;

    public UserQueryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity?> GetUserByEmail(string email)
    {
        var dto = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        return dto?.ToDomain();
    }

    public async Task<UserEntity?> GetUserByName(string name)
    {
        var dto = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Name == name);

        return dto?.ToDomain();
    }
}