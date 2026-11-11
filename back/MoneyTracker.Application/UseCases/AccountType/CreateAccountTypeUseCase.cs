using MoneyTracker.Domain.Application.AccountType;
using MoneyTracker.Domain.Entities.types;
using MoneyTracker.Domain.Repository;

namespace ClassLibrary1MoneyTracker.Application.UseCases.AccountType;

public class CreateAccountTypeUseCase : ICreateAccountTypeUseCase
{
    private readonly IAccountTypeRepository _accountTypeRepository;

    
    public CreateAccountTypeUseCase( IAccountTypeRepository accountTypeRepository)
    {
        _accountTypeRepository = accountTypeRepository;
    }

    public async Task<AccountTypeEntity> ExecuteAsync(string name)
    {
        return await _accountTypeRepository.CreateAsync(name);
    }
}