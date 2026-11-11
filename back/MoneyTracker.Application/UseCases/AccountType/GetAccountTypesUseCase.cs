using MoneyTracker.Domain.Application.AccountType;
using MoneyTracker.Domain.Entities.types;
using MoneyTracker.Domain.QueryService;

namespace ClassLibrary1MoneyTracker.Application.UseCases.AccountType;

public class GetAccountTypesUseCase : IGetAccountTypesUseCase
{
    private readonly IAccountTypeQueryService _accountTypeQueryService;

    public GetAccountTypesUseCase(IAccountTypeQueryService accountTypeQueryService)
    {
        _accountTypeQueryService = accountTypeQueryService;
    }

    public Task<List<AccountTypeEntity>> ExecuteAsync()
    {
        return _accountTypeQueryService.GetAllAsync();
    }
}
