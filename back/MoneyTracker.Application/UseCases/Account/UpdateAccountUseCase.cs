using MoneyTracker.Domain.Application.Account;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;

namespace ClassLibrary1MoneyTracker.Application.UseCases.Account;

public class UpdateAccountUseCase : IUpdateAccountUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IGetAccountByIdUseCase _getAccountByIdUseCase;
    

    public UpdateAccountUseCase(IAccountRepository accountRepository, IGetAccountByIdUseCase getAccountByIdUseCase)
    {
        _accountRepository = accountRepository;
        _getAccountByIdUseCase = getAccountByIdUseCase;
        
    }

    public async Task<AccountEntity?> ExecuteAsync(string? name, int? accountTypeId, decimal? balance, int id, int userId)
    {
        var tracked = await _getAccountByIdUseCase.ExecuteAsync(id, userId);
        if (tracked is null) return null;

        // NAME
        if (!string.IsNullOrWhiteSpace(name) && name != tracked.Name)
            tracked.Name = name;

        // ACCOUNT TYPE
        if (accountTypeId is { } atId && atId > 0 && atId != tracked.AccountTypeId)
            tracked.AccountTypeId = atId;

        // BALANCE
        if (balance.HasValue && balance.Value != tracked.Balance)
            tracked.Balance = balance.Value;

        return await _accountRepository.UpdateAsync(tracked);
    }
}