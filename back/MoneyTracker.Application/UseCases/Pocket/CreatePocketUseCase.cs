using MoneyTracker.Domain.Application.Pocket;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Pocket;

public class CreatePocketUseCase : ICreatePocketUseCase
{
    private readonly IPocketRepository _pocketRepository;

    public CreatePocketUseCase(IPocketRepository pocketRepository)
    {
        _pocketRepository = pocketRepository;
    }

    public Task<PocketEntity> ExecuteAsync(int userId, int accountId, string name, decimal balance)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El pocket necesita un nombre.");
        if (balance < 0)
            throw new ArgumentException("El saldo inicial del pocket no puede ser negativo.");

        var pocket = new PocketEntity
        {
            UserId = userId,
            AccountId = accountId,
            Name = name,
            Balance = balance
        };
        return _pocketRepository.CreateAsync(pocket);
    }
}
