using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Infrastructure.MySql.Repository.Pet;

public class PetRepository : IPetRepository
{
    private readonly AppDbContext _context;

    public PetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PetEntity> CreateAsync(PetEntity entity)
    {
        // El id viene puesto (es el de PetRecording): la tabla no lo autogenera,
        // y en el DbContext esta marcado como ValueGeneratedNever.
        var dto = new PetDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Name = entity.Name
        };

        await _context.Pets.AddAsync(dto);
        await _context.SaveChangesAsync();
        return dto.ToDomain();
    }
}
