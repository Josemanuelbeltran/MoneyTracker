using MoneyTracker.Domain.Application.Transfer;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.Transfer;

public class GetTransfersUseCase : IGetTransfersUseCase
{
    private readonly ITransferQueryService _transferQueryService;

    public GetTransfersUseCase(ITransferQueryService transferQueryService)
    {
        _transferQueryService = transferQueryService;
    }

    public Task<List<TransferEntity>> ExecuteAsync(int userId)
    {
        return _transferQueryService.GetByUserAsync(userId);
    }
}
