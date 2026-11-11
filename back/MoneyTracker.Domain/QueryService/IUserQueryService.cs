using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.QueryService;

public interface IUserQueryService
{
    Task<UserEntity?> GetUserByEmail(string email);

    Task<UserEntity?> GetUserByName(string name);
}