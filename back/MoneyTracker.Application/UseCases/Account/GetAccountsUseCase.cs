using MoneyTracker.Domain.Application.Account;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.Account;

public class GetAccountsUseCase : IGetAccountsUseCase
{
    private readonly IAccountQueryService _accountQueryService;

    public GetAccountsUseCase(IAccountQueryService accountQueryService)
    {
        _accountQueryService = accountQueryService;
    }

    public Task<List<AccountEntity>> ExecuteAsync(int userId)
    {
        return _accountQueryService.GetAllByUserIdAsync(userId);
    }
}
