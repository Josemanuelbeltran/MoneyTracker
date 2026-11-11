using MoneyTracker.Domain.Application.AccountType;
using MoneyTracker.Domain.Repository;

namespace ClassLibrary1MoneyTracker.Application.UseCases.AccountType;

public class DeleteAccountTypeByIdUseCase : IDeleteAccountTypeByIdUseCase
{
    private readonly IAccountTypeRepository _accountTypeRepository;

    public DeleteAccountTypeByIdUseCase(IAccountTypeRepository accountTypeRepository)
    {
        _accountTypeRepository = accountTypeRepository;
    }

    public async Task<bool> ExecuteAsync(int id)
    {
        return await _accountTypeRepository.DeleteAsync(id);
    }
}