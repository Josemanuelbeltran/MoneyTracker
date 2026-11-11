using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Repository;

public interface IUserRepository
{
    Task<UserEntity> CreateAsync(string name, string email, string passwordHash, string? phoneNumber , bool? verifiedPhone );
}