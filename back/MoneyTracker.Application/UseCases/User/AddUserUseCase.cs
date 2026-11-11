using MoneyTracker.Domain.Application.User;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;

namespace ClassLibrary1MoneyTracker.Application.UseCases.User;

public class AddUserUseCase : IAddUserUseCase
{
    private readonly IUserRepository _repository;

    public AddUserUseCase(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserEntity> ExecuteAsync(string name, string email, string passwordHash, string? phoneNumber, bool? verifiedPhone)
    {
        var user = await _repository.CreateAsync(name,email, passwordHash, phoneNumber, verifiedPhone);
        return user;

    }
}