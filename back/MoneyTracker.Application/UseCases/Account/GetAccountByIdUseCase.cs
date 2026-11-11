using MoneyTracker.Domain.Application.Account;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.Account;

public class GetAccountByIdUseCase : IGetAccountByIdUseCase
{
    private IAccountQueryService _accountQueryService;

    public GetAccountByIdUseCase(IAccountQueryService accountQueryService)
    {
        _accountQueryService = accountQueryService;
    }

    public async Task<AccountEntity?> ExecuteAsync(int id, int userId)
    {
        var account = await _accountQueryService.GetByIdAsync(id, userId);
        return account;
    }
}