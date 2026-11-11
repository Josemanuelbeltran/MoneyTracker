using MoneyTracker.Domain.Application.Recurring;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Recurring;

public class GenerateRecurringUseCase : IGenerateRecurringUseCase
{
    private readonly IIncomeQueryService _incomeQuery;
    private readonly IExpenseQueryService _expenseQuery;
    private readonly IIncomeRepository _incomeRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IPocketRepository _pocketRepository;

    public GenerateRecurringUseCase(
        IIncomeQueryService incomeQuery,
        IExpenseQueryService expenseQuery,
        IIncomeRepository incomeRepository,
        IExpenseRepository expenseRepository,
        IPocketRepository pocketRepository)
    {
        _incomeQuery = incomeQuery;
        _expenseQuery = expenseQuery;
        _incomeRepository = incomeRepository;
        _expenseRepository = expenseRepository;
        _pocketRepository = pocketRepository;
    }

    public async Task<int> ExecuteAsync(int userId)
    {
        var now = DateTime.Now;
        var created = 0;

        // --- Ingresos fijos ---
        var incomes = await _incomeQuery.GetByUserAsync(userId);
        // La serie NO incluye el importe: si se incluyera, al subir el importe desde una
        // fecha ("editar este y siguientes") el grupo del importe viejo se quedaría sin
        // ocurrencia este mes y se regeneraría, duplicando el movimiento.
        var incomeGroups = incomes
            .Where(i => i.IsRecurring)
            .GroupBy(i => new { i.AccountId, i.CategoryId, i.Description });

        foreach (var group in incomeGroups)
        {
            var existsThisMonth = group.Any(i =>
                i.IncomeDate.Year == now.Year && i.IncomeDate.Month == now.Month);

            if (existsThisMonth)
                continue;

            var template = group.OrderByDescending(i => i.IncomeDate).First();
            await _incomeRepository.CreateAsync(new IncomeEntity
            {
                UserId = userId,
                AccountId = template.AccountId,
                CategoryId = template.CategoryId,
                Amount = template.Amount,
                Description = template.Description,
                IsRecurring = true,
                IncomeDate = new DateOnly(now.Year, now.Month, 1),
            });
            created++;
        }

        // --- Gastos fijos ---
        var expenses = await _expenseQuery.GetByUserAsync(userId);
        // Mismo criterio que en ingresos: la serie es cuenta + categoría + descripción.
        var expenseGroups = expenses
            .Where(e => e.IsRecurring)
            .GroupBy(e => new { e.AccountId, e.CategoryId, e.Description });

        foreach (var group in expenseGroups)
        {
            var existsThisMonth = group.Any(e =>
                e.ExpenseDate.Year == now.Year && e.ExpenseDate.Month == now.Month);

            if (existsThisMonth)
                continue;

            var template = group.OrderByDescending(e => e.ExpenseDate).First();
            await _expenseRepository.CreateAsync(new ExpenseEntity
            {
                UserId = userId,
                AccountId = template.AccountId,
                CategoryId = template.CategoryId,
                Amount = template.Amount,
                Description = template.Description,
                IsRecurring = true,
                ExpenseDate = new DateTime(now.Year, now.Month, 1),
                // El fijo hereda el pocket de la plantilla: si el alquiler sale del sobre
                // "Hogar", la copia de este mes también. Antes se creaba sin pocket, así
                // que bajaba el saldo de la cuenta pero el sobre no se enteraba.
                PocketId = template.PocketId,
            });

            // Y hay que descontarlo del pocket, igual que al crear el gasto a mano.
            if (template.PocketId is int pocketId)
            {
                try
                {
                    await _pocketRepository.MoveAsync(pocketId, null, template.Amount, userId);
                }
                catch (KeyNotFoundException)
                {
                    // El pocket de la plantilla ya no existe: el gasto queda sin sobre.
                }
            }

            created++;
        }

        return created;
    }
}
