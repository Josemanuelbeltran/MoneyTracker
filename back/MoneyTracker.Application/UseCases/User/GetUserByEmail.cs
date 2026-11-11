using MoneyTracker.Domain.Application.User;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace ClassLibrary1MoneyTracker.Application.UseCases.User;

public class GetUserByEmail : IGetUserByEmailUseCase
{
    private readonly IUserQueryService _userQueryService;

    public GetUserByEmail(IUserQueryService userQueryService)
    {
        _userQueryService = userQueryService;
    }

    public async Task<UserEntity?> ExecuteAsync(string email)
    {
        return await _userQueryService.GetUserByEmail(email);
         
    }
}