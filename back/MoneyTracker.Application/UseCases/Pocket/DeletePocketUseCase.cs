using MoneyTracker.Domain.Application.Pocket;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Pocket;

public class DeletePocketUseCase : IDeletePocketUseCase
{
    private readonly IPocketRepository _pocketRepository;

    public DeletePocketUseCase(IPocketRepository pocketRepository)
    {
        _pocketRepository = pocketRepository;
    }

    public Task<bool> ExecuteAsync(int id, int userId)
    {
        return _pocketRepository.DeleteAsync(id, userId);
    }
}
