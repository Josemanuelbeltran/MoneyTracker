using MoneyTracker.Domain.Application.Pocket;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.Pocket;

public class GetPocketsUseCase : IGetPocketsUseCase
{
    private readonly IPocketQueryService _pocketQueryService;

    public GetPocketsUseCase(IPocketQueryService pocketQueryService)
    {
        _pocketQueryService = pocketQueryService;
    }

    public Task<List<PocketEntity>> ExecuteAsync(int userId)
    {
        return _pocketQueryService.GetByUserAsync(userId);
    }
}
