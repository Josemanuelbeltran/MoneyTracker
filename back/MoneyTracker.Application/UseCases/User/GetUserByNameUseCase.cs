using MoneyTracker.Domain.Application.User;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace ClassLibrary1MoneyTracker.Application.UseCases.User;

public class GetUserByNameUseCase : IGetUserByNameUseCase
{
    private readonly IUserQueryService _userQueryService;

    public GetUserByNameUseCase(IUserQueryService userQueryService)
    {
        _userQueryService = userQueryService;
    }

    public async Task<UserEntity?> ExecuteAsync(string name)
    {
        return await _userQueryService.GetUserByName(name);
    }
}
