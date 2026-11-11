using MoneyTracker.Domain.Application.Pocket;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Pocket;

public class UpdatePocketUseCase : IUpdatePocketUseCase
{
    private readonly IPocketRepository _pocketRepository;

    public UpdatePocketUseCase(IPocketRepository pocketRepository)
    {
        _pocketRepository = pocketRepository;
    }

    public Task<PocketEntity?> ExecuteAsync(int id, string name, int userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El pocket necesita un nombre.");

        return _pocketRepository.UpdateAsync(id, name, userId);
    }
}
