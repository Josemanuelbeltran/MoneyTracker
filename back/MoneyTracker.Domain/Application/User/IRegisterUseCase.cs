using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.User;

public interface IRegisterUseCase
{
    Task<UserEntity> ExecuteAsync(string name, string email, string passwordHash, string? phoneNumber = null, bool? verifiedPhone = false);
}