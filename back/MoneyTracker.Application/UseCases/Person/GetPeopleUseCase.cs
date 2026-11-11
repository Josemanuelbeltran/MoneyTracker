using MoneyTracker.Domain.Application.Person;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.UseCases.Person;

public class GetPeopleUseCase : IGetPeopleUseCase
{
    private readonly IPersonQueryService _queryService;

    public GetPeopleUseCase(IPersonQueryService queryService)
    {
        _queryService = queryService;
    }

    public Task<List<PersonEntity>> ExecuteAsync(int userId)
    {
        return _queryService.GetByUserAsync(userId);
    }
}
