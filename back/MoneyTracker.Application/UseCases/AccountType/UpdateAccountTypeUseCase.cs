using MoneyTracker.Domain.Application.AccountType;
using MoneyTracker.Domain.Repository;

namespace ClassLibrary1MoneyTracker.Application.UseCases.AccountType;

public class UpdateAccountTypeUseCase : IUpdateAccountTypeUseCase
{
    private readonly IAccountTypeRepository _repository;

    public UpdateAccountTypeUseCase(IAccountTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> ExecuteAsync(int id, string name)
    {
        var accountType = await _repository.UpdateAsync(id, name);
        if (accountType)
        {
            return true;
        }
        return false;
    }
}