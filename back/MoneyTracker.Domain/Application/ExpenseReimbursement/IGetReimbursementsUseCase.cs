using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Domain.Application.ExpenseReimbursement;

public interface IGetReimbursementsUseCase
{
    Task<List<ExpenseReimbursementEntity>> ExecuteAsync(int userId);
}
