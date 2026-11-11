using MoneyTracker.Domain.Application.Pet;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.Pet;

public class GetPetsUseCase : IGetPetsUseCase
{
    private readonly IPetQueryService _queryService;

    public GetPetsUseCase(IPetQueryService queryService)
    {
        _queryService = queryService;
    }

    public Task<List<PetEntity>> ExecuteAsync(int userId)
    {
        return _queryService.GetByUserAsync(userId);
    }
}
