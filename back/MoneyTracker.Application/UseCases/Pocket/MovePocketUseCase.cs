using MoneyTracker.Domain.Application.Pocket;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Pocket;

public class MovePocketUseCase : IMovePocketUseCase
{
    private readonly IPocketRepository _pocketRepository;

    public MovePocketUseCase(IPocketRepository pocketRepository)
    {
        _pocketRepository = pocketRepository;
    }

    public Task ExecuteAsync(int? fromPocketId, int? toPocketId, decimal amount, int userId)
    {
        if (amount <= 0)
            throw new ArgumentException("El importe a mover debe ser mayor que 0.");
        if (fromPocketId is null && toPocketId is null)
            throw new ArgumentException("Indica al menos un pocket de origen o destino.");
        if (fromPocketId is not null && fromPocketId == toPocketId)
            throw new ArgumentException("El pocket de origen y destino no pueden ser el mismo.");

        return _pocketRepository.MoveAsync(fromPocketId, toPocketId, amount, userId);
    }
}
