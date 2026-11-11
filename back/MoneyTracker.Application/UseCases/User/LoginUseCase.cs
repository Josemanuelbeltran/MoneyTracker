using MoneyTracker.Domain.Application.Security;
using MoneyTracker.Domain.Application.User;
using MoneyTracker.Domain.Entities;

namespace ClassLibrary1MoneyTracker.Application.UseCases.User;

public class LoginUseCase : ILoginUseCase
{
    private readonly IGetUserByNameUseCase _getUserByNameUseCase;
    private readonly IJwtTokenGenerator _jWTokenGenerator;


    public LoginUseCase(IGetUserByNameUseCase getUserByNameUseCase, IJwtTokenGenerator jWTokenGenerator)
    {
        _getUserByNameUseCase = getUserByNameUseCase;
        _jWTokenGenerator = jWTokenGenerator;
    }

    public async Task<string?> ExecuteAsync(string username, string password)
    {
        var user = await _getUserByNameUseCase.ExecuteAsync(username);
        if (user is not null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            var token = _jWTokenGenerator.GenerateJwtToken(user.Id.ToString(), user.IsAdmin);
            return token;
        }
        return null;
    }
}