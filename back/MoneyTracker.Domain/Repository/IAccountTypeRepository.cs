using MoneyTracker.Domain.Entities.types;

namespace MoneyTracker.Domain.Repository;

public interface IAccountTypeRepository
{
    Task <AccountTypeEntity> CreateAsync(string name);
    Task<bool> DeleteAsync(int id);
    
    Task<bool> UpdateAsync(int id, string name);
}