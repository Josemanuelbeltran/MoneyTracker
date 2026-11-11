using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.Repository.Person;

public class PersonRepository : IPersonRepository
{
    private readonly AppDbContext _context;

    public PersonRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PersonEntity> CreateAsync(PersonEntity entity)
    {
        var dto = new PersonDto
        {
            UserId = entity.UserId,
            Name = entity.Name,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _context.People.AddAsync(dto);
        await _context.SaveChangesAsync();
        return dto.ToDomain();
    }

    public async Task<bool> UpdateAsync(int id, int userId, string name)
    {
        var rows = await _context.People
            .Where(x => x.Id == id && x.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Name, name)
                .SetProperty(x => x.UpdatedAt, DateTime.Now));

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var rows = await _context.People
            .Where(x => x.Id == id && x.UserId == userId)
            .ExecuteDeleteAsync();

        return rows > 0;
    }

    public async Task<PersonEntity?> GetByNameAsync(int userId, string name)
    {
        var dto = await _context.People
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Name == name);

        return dto?.ToDomain();
    }
}
