using MoneyTracker.Domain.Application.Account;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;

namespace ClassLibrary1MoneyTracker.Application.UseCases.Account;

public class CreateAccountUseCase : ICreateAccountUseCase
{
    private readonly IAccountRepository _accountRepository;

    public CreateAccountUseCase(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public Task<AccountEntity?> ExecuteAsync(AccountEntity entity)
    {
        var account = _accountRepository.CreateAsync(entity);
        return account;
    }
}