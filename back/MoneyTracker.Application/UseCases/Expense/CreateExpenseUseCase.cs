using MoneyTracker.Domain.Application.Expense;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Integration;
using MoneyTracker.Domain.Repository;

namespace ClassLibrary1MoneyTracker.Application.UseCases.Expense;

public class CreateExpenseUseCase : ICreateExpenseUseCase
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IPocketRepository _pocketRepository;
    private readonly IPetExpenseNotifier _petNotifier;

    public CreateExpenseUseCase(
        IExpenseRepository expenseRepository,
        IPocketRepository pocketRepository,
        IPetExpenseNotifier petNotifier)
    {
        _expenseRepository = expenseRepository;
        _pocketRepository = pocketRepository;
        _petNotifier = petNotifier;
    }

    public async Task<ExpenseEntity> ExecuteAsync(int userId, int accountId, int categoryId, decimal amount, string description, DateTime expenseDate, int? pocketId = null, int? petId = null)
    {
        // Si el gasto sale de un pocket, se descuenta de ese pocket (valida que tenga saldo).
        if (pocketId is int pid)
            await _pocketRepository.MoveAsync(pid, null, amount, userId);

        var expense = new ExpenseEntity
        {
            UserId = userId,
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = amount,
            Description = description,
            ExpenseDate = expenseDate,
            PocketId = pocketId,
            PetId = petId
        };
        var created = await _expenseRepository.CreateAsync(expense);

        // Gasto de mascota: se lo contamos a PetRecording. Si está caída, el gasto
        // ya está guardado y el fallo solo queda en el log.
        if (created.PetId is not null)
        {
            try
            {
                await _petNotifier.ExpenseSavedAsync(created);
            }
            catch
            {
                // El notificador ya traga y registra sus propios errores.
            }
        }

        return created;
    }
}
