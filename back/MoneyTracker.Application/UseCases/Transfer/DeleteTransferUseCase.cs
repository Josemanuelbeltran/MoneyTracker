using MoneyTracker.Domain.Application.Transfer;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Transfer;

public class DeleteTransferUseCase : IDeleteTransferUseCase
{
    private readonly ITransferRepository _transferRepository;

    public DeleteTransferUseCase(ITransferRepository transferRepository)
    {
        _transferRepository = transferRepository;
    }

    public Task<bool> ExecuteAsync(int id, int userId)
    {
        return _transferRepository.DeleteAsync(id, userId);
    }
}
