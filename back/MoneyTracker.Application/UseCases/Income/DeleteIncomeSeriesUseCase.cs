using MoneyTracker.Domain.Application.Income;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Income;

public class DeleteIncomeSeriesUseCase : IDeleteIncomeSeriesUseCase
{
    private readonly IIncomeQueryService _incomeQuery;
    private readonly IIncomeRepository _incomeRepository;

    public DeleteIncomeSeriesUseCase(IIncomeQueryService incomeQuery, IIncomeRepository incomeRepository)
    {
        _incomeQuery = incomeQuery;
        _incomeRepository = incomeRepository;
    }

    public async Task<int> ExecuteAsync(int id, int userId)
    {
        var anchor = await _incomeQuery.GetByIdAsync(id, userId)
                     ?? throw new KeyNotFoundException("Income no existe");

        if (!anchor.IsRecurring)
            throw new ArgumentException("El ingreso no es recurrente: no forma parte de una serie.");

        // Misma clave de serie que en la edición: usuario + cuenta + categoría, desde la
        // fecha del ancla hacia delante (el histórico anterior se conserva).
        return await _incomeRepository.DeleteSeriesForwardAsync(
            userId, anchor.AccountId, anchor.CategoryId, anchor.IncomeDate);
    }
}
