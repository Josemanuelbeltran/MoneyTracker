using MoneyTracker.Domain.Application.Expense;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Integration;
using MoneyTracker.Domain.Repository;

namespace MoneyTracker.Application.UseCases.Expense;

public class CreateSplitSpreadExpenseUseCase : ICreateSplitSpreadExpenseUseCase
{
    private const int MaxSpreadMonths = 60;
    private const int MaxSplitParts = 100;

    private readonly IExpenseRepository _expenseRepository;
    private readonly IPocketRepository _pocketRepository;
    private readonly IExpenseReimbursementRepository _reimbursementRepository;
    private readonly IPetExpenseNotifier _petNotifier;

    public CreateSplitSpreadExpenseUseCase(
        IExpenseRepository expenseRepository,
        IPocketRepository pocketRepository,
        IExpenseReimbursementRepository reimbursementRepository,
        IPetExpenseNotifier petNotifier)
    {
        _expenseRepository = expenseRepository;
        _pocketRepository = pocketRepository;
        _reimbursementRepository = reimbursementRepository;
        _petNotifier = petNotifier;
    }

    public async Task<List<ExpenseEntity>> ExecuteAsync(
        ExpenseEntity template,
        int? splitParts,
        int spreadMonths = 1,
        decimal? myShareAmount = null,
        int? reimburseToAccountId = null,
        string? debtor = null,
        int? personId = null,
        IReadOnlyList<int?>? debtorPersonIds = null,
        IReadOnlyList<string?>? debtorNames = null)
    {
        var total = template.Amount;
        if (total <= 0)
            throw new ArgumentException("El importe debe ser mayor que 0.");
        if (splitParts is not null && myShareAmount is not null)
            throw new ArgumentException("Elige un reparto: a partes iguales o indicando tu parte, no los dos.");
        if (splitParts is int p && (p < 2 || p > MaxSplitParts))
            throw new ArgumentException($"El número de partes debe estar entre 2 y {MaxSplitParts}.");
        if (myShareAmount is decimal share && (share <= 0 || share > total))
            throw new ArgumentException("Tu parte debe ser mayor que 0 y no pasar del total.");
        if (spreadMonths < 1 || spreadMonths > MaxSpreadMonths)
            throw new ArgumentException($"El número de meses debe estar entre 1 y {MaxSpreadMonths}.");

        // 1) Reparto entre personas. A partes iguales el usuario paga UNA de N;
        //    con reparto libre paga exactamente lo que ha dicho (un 70/30).
        //    En ambos casos el resto es de otra gente, así que basta con redondear a céntimos.
        var isEqualSplit = splitParts is > 1;
        var myShare = isEqualSplit
            ? Math.Round(total / splitParts!.Value, 2, MidpointRounding.AwayFromZero)
            : Math.Round(myShareAmount ?? total, 2, MidpointRounding.AwayFromZero);

        var isSplit = isEqualSplit || myShareAmount is not null;

        // 2) Lo que adelantó de los demás. Solo se registra si eligió cuenta de destino
        //    y de verdad hay algo que le devuelvan.
        var owedToMe = total - myShare;
        var hasReimbursement = reimburseToAccountId is not null && owedToMe > 0;

        // A partes iguales puede decir QUIÉN le debe cada parte: una persona por cada
        // parte que no es suya. Si no lo dice, queda un único reembolso sin dueño.
        var debtors = debtorPersonIds?.ToList() ?? new List<int?>();
        if (debtors.Count > 0 && isEqualSplit && debtors.Count != splitParts!.Value - 1)
            throw new ArgumentException(
                $"Con {splitParts} partes hacen falta {splitParts - 1} personas, y han llegado {debtors.Count}.");

        // 3) Prorrateo en meses: aquí TODAS las cuotas son del usuario, así que deben sumar
        //    exactamente su parte. La última absorbe el resto (33,33 / 33,33 / 33,34).
        var isSpread = spreadMonths > 1;
        var spreadGroup = isSpread ? Guid.NewGuid().ToString() : null;
        var baseInstalment = Math.Floor(myShare / spreadMonths * 100m) / 100m;

        // Del pocket sale el TOTAL: tu parte y lo que adelantas de los demás. El pocket
        // es de donde sacas el dinero, y de ahí salió todo. Se descuenta ANTES de crear
        // nada: si fallara, no queremos dejar el gasto guardado y al usuario viendo un
        // error, creyendo que no se guardó y metiéndolo otra vez.
        if (hasReimbursement && template.PocketId is int advancePocketId)
            await _pocketRepository.MoveAsync(advancePocketId, null, owedToMe, template.UserId);

        var created = new List<ExpenseEntity>(spreadMonths);

        for (var i = 0; i < spreadMonths; i++)
        {
            var isLast = i == spreadMonths - 1;
            var amount = isLast
                ? myShare - baseInstalment * (spreadMonths - 1)
                : baseInstalment;

            var expense = new ExpenseEntity
            {
                UserId = template.UserId,
                AccountId = template.AccountId,
                CategoryId = template.CategoryId,
                Amount = amount,
                Description = template.Description,
                // Siempre desde la fecha original para que el día no se desplace.
                ExpenseDate = template.ExpenseDate.AddMonths(i),
                IsRecurring = template.IsRecurring,
                PocketId = template.PocketId,
                PetId = template.PetId,
                TotalAmount = isSplit ? total : null,
                // Solo el reparto a partes iguales guarda el nº de partes; con reparto
                // libre queda a NULL y el porcentaje sale de amount / total_amount.
                SplitParts = isEqualSplit ? splitParts : null,
                SpreadGroup = spreadGroup,
                SpreadIndex = isSpread ? i + 1 : null,
                SpreadCount = isSpread ? spreadMonths : null,
            };

            // Si el gasto sale de un pocket, cada cuota se descuenta de ese pocket.
            if (template.PocketId is int pocketId)
                await _pocketRepository.MoveAsync(pocketId, null, amount, template.UserId);

            created.Add(await _expenseRepository.CreateAsync(expense));
        }

        if (hasReimbursement)
        {
            // Los adelantos van colgados de la primera cuota: el dinero salió una sola
            // vez, aunque tu parte se reparta en varios meses.
            var expenseId = created[0].Id;

            if (debtors.Count > 0)
            {
                // Una deuda por persona. Se reparte lo adelantado entre ellas y la última
                // absorbe el resto, para que la suma cuadre con el total al céntimo.
                var each = Math.Floor(owedToMe / debtors.Count * 100m) / 100m;

                for (var i = 0; i < debtors.Count; i++)
                {
                    var isLastDebtor = i == debtors.Count - 1;
                    var debtorShare = isLastDebtor ? owedToMe - each * (debtors.Count - 1) : each;

                    await _reimbursementRepository.CreateAsync(new ExpenseReimbursementEntity
                    {
                        UserId = template.UserId,
                        ExpenseId = expenseId,
                        Amount = debtorShare,
                        ToAccountId = reimburseToAccountId!.Value,
                        PersonId = debtors[i],
                        Debtor = debtorNames is not null && i < debtorNames.Count ? debtorNames[i] : null,
                    });
                }
            }
            else
            {
                await _reimbursementRepository.CreateAsync(new ExpenseReimbursementEntity
                {
                    UserId = template.UserId,
                    ExpenseId = expenseId,
                    Amount = owedToMe,
                    ToAccountId = reimburseToAccountId!.Value,
                    Debtor = string.IsNullOrWhiteSpace(debtor) ? null : debtor.Trim(),
                    PersonId = personId,
                });
            }
        }

        // Gastos de mascota: uno por cuota, cada uno con su id. Si PetRecording esta
        // caida el gasto ya esta guardado y el fallo solo queda en el log.
        foreach (var expense in created.Where(x => x.PetId is not null))
        {
            try
            {
                await _petNotifier.ExpenseSavedAsync(expense);
            }
            catch
            {
                // El notificador ya traga y registra sus propios errores.
            }
        }

        return created;
    }
}
