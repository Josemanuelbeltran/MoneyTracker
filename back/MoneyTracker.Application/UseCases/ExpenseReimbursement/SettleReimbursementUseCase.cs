using MoneyTracker.Domain.Application.ExpenseReimbursement;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.ExpenseReimbursement;

public class SettleReimbursementUseCase : ISettleReimbursementUseCase
{
    private readonly IExpenseReimbursementRepository _repository;

    public SettleReimbursementUseCase(IExpenseReimbursementRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> ExecuteAsync(int id, int userId, bool settled)
    {
        return settled
            ? _repository.SettleAsync(id, userId, DateTime.Now)
            : _repository.UnsettleAsync(id, userId);
    }
}
