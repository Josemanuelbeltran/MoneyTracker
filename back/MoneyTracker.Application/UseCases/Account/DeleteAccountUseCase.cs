using MoneyTracker.Domain.Application.Account;
using MoneyTracker.Domain.Repository;

namespace ClassLibrary1MoneyTracker.Application.UseCases.Account;

public class DeleteAccountUseCase : IDeleteAccountUseCase
{
    private readonly IAccountRepository _accountRepository;

    public DeleteAccountUseCase(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public Task<bool?> ExecuteAsync(int id, int userId)
    {
        return _accountRepository.DeleteAsync(id, userId);
    }
}