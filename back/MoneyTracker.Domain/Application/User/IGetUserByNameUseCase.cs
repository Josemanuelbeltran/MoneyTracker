using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.User;

public interface IGetUserByNameUseCase
{
    Task<UserEntity?> ExecuteAsync(string name);
}
