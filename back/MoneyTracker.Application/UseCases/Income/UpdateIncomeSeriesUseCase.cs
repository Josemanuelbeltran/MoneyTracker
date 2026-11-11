using MoneyTracker.Domain.Application.Income;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Income;

public class UpdateIncomeSeriesUseCase : IUpdateIncomeSeriesUseCase
{
    private readonly IIncomeQueryService _incomeQuery;
    private readonly IIncomeRepository _incomeRepository;

    public UpdateIncomeSeriesUseCase(IIncomeQueryService incomeQuery, IIncomeRepository incomeRepository)
    {
        _incomeQuery = incomeQuery;
        _incomeRepository = incomeRepository;
    }

    public async Task<int> ExecuteAsync(int id, int userId, decimal newAmount)
    {
        if (newAmount <= 0)
            throw new ArgumentException("El importe debe ser mayor que 0.");

        var anchor = await _incomeQuery.GetByIdAsync(id, userId)
                     ?? throw new KeyNotFoundException("Income no existe");

        if (!anchor.IsRecurring)
            throw new ArgumentException("El ingreso no es recurrente: no forma parte de una serie.");

        // La serie se identifica por usuario + cuenta + categoría; el ingreso ancla marca
        // el corte temporal, así que el histórico anterior a su fecha no se toca.
        return await _incomeRepository.UpdateAmountForwardAsync(
            userId, anchor.AccountId, anchor.CategoryId, anchor.IncomeDate, newAmount);
    }
}
