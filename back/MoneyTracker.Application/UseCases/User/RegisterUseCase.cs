using MoneyTracker.Domain.Application.User;
using MoneyTracker.Domain.Entities;

namespace ClassLibrary1MoneyTracker.Application.UseCases.User;

public class RegisterUseCase : IRegisterUseCase
{
    private readonly IAddUserUseCase _addUserUseCase;

    public RegisterUseCase(IAddUserUseCase addUserUseCase)
    {
        _addUserUseCase = addUserUseCase;
    }

    public async Task<UserEntity> ExecuteAsync(string name,string email, string passwordHash, string? phoneNumber , bool? verifiedPhone)
    {
        var user = await _addUserUseCase.ExecuteAsync(name, email, BCrypt.Net.BCrypt.HashPassword(passwordHash), phoneNumber, verifiedPhone); 
        return user;
    }
    
}