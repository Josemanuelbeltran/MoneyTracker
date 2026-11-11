using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.User;

public interface IGetUserByEmailUseCase
{
 Task <UserEntity?> ExecuteAsync(string email);  
}