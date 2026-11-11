using MoneyTracker.Domain.Application.Income;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Income;

public class CreateRecurringIncomeUseCase : ICreateRecurringIncomeUseCase
{
    private const int MaxMonthsAhead = 60;

    private readonly IIncomeRepository _incomeRepository;

    public CreateRecurringIncomeUseCase(IIncomeRepository incomeRepository)
    {
        _incomeRepository = incomeRepository;
    }

    public async Task<int> ExecuteAsync(IncomeEntity template, int monthsAhead = 12)
    {
        if (monthsAhead < 1)
            monthsAhead = 1;
        if (monthsAhead > MaxMonthsAhead)
            monthsAhead = MaxMonthsAhead;

        var created = 0;

        for (var i = 0; i < monthsAhead; i++)
        {
            // Siempre desde la fecha original para que no se desplace el día:
            // AddMonths ya recorta al último día del mes (31 ene + 1 mes = 28 feb).
            var occurrenceDate = template.IncomeDate.AddMonths(i);

            await _incomeRepository.CreateAsync(new IncomeEntity
            {
                UserId = template.UserId,
                AccountId = template.AccountId,
                CategoryId = template.CategoryId,
                Amount = template.Amount,
                Description = template.Description,
                IsRecurring = true,
                IncomeDate = occurrenceDate,
            });
            created++;
        }

        return created;
    }
}
