using MoneyTracker.Domain.Application.Transfer;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Transfer;

public class CreateTransferUseCase : ICreateTransferUseCase
{
    private readonly ITransferRepository _transferRepository;

    public CreateTransferUseCase(ITransferRepository transferRepository)
    {
        _transferRepository = transferRepository;
    }

    public Task<TransferEntity> ExecuteAsync(int userId, int fromAccountId, int toAccountId, decimal amount, string? description, DateTime transferDate)
    {
        if (amount <= 0)
            throw new ArgumentException("El importe de la transferencia debe ser mayor que 0.");
        if (fromAccountId == toAccountId)
            throw new ArgumentException("La cuenta de origen y destino no pueden ser la misma.");

        var transfer = new TransferEntity
        {
            UserId = userId,
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            Description = description,
            TransferDate = transferDate
        };
        return _transferRepository.CreateAsync(transfer);
    }
}
