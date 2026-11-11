using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.User;

public interface IAddUserUseCase
{
    Task<UserEntity> ExecuteAsync(string name,string email, string passwordHash, string? phoneNumber, bool? verifiedPhone);
}