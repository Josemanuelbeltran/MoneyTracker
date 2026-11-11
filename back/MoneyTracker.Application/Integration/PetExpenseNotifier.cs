using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Integration;
using MoneyTracker.Domain.QueryService;

namespace MoneyTracker.Application.Integration;

public class PetExpenseNotifier : IPetExpenseNotifier
{
    private readonly IPetRecordingClient _client;
    private readonly IExpenseCategoryQueryService _categoryQueryService;

    public PetExpenseNotifier(IPetRecordingClient client, IExpenseCategoryQueryService categoryQueryService)
    {
        _client = client;
        _categoryQueryService = categoryQueryService;
    }

    public async Task ExpenseSavedAsync(ExpenseEntity expense, CancellationToken cancellationToken = default)
    {
        if (expense.PetId is not int petId) return;

        // El reparto sale de lo que ya hay, sin inventar nada: el importe del gasto
        // es tu parte (regla de toda la app), el total es el del gasto compartido y
        // lo que falta es de la familia. En un gasto normal, familia = 0.
        var ownerAmount = expense.Amount;
        var totalAmount = expense.TotalAmount ?? expense.Amount;
        var familyAmount = totalAmount - ownerAmount;

        var category = await _categoryQueryService.GetByIdAsync(expense.CategoryId, expense.UserId);

        await _client.UpsertExpenseAsync(
            new PetExpenseSync(
                MtExpenseId: expense.Id,
                PetId: petId,
                TotalAmount: totalAmount,
                OwnerAmount: ownerAmount,
                FamilyAmount: familyAmount,
                Date: DateOnly.FromDateTime(expense.ExpenseDate),
                Category: category?.Name ?? "Mascotas",
                Description: expense.Description),
            cancellationToken);
    }

    public Task ExpenseDeletedAsync(int expenseId, CancellationToken cancellationToken = default)
    {
        return _client.DeleteExpenseAsync(expenseId, cancellationToken);
    }
}
