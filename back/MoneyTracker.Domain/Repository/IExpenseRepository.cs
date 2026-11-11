using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Repository;

public interface IExpenseRepository
{
    Task<ExpenseEntity> CreateAsync(ExpenseEntity entity);

    /// <summary>Borra un gasto del usuario. Si no es suyo, no borra nada.</summary>
    Task<bool> DeleteAsync(int id, int userId);

    /// <summary>Actualiza un gasto del usuario (la entidad lleva su UserId).</summary>
    Task<ExpenseEntity> UpdateAsync(ExpenseEntity entity);
}
