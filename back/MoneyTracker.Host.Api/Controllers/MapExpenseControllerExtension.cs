using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Application.Expense;
using MoneyTracker.Host.Api.Security;
using MoneyTracker.Infrastructure.MySql.DDBBConection;
using MoneyTracker.Infrastructure.MySql.Dtos;
using MoneyTracker.Infrastructure.MySql.Mappers;

namespace MoneyTracker.Host.Api.Controllers;

public static class MapExpenseControllerExtension
{
    public static void AddExpenseController(this IEndpointRouteBuilder app)
    {
        app.MapPost("/expenses", async (
                [FromServices] ICreateExpenseUseCase useCase,
                AppDbContext db,
                [FromBody] ExpenseDto body,
                HttpContext ctx) =>
            {
                var userId = ctx.GetUserId();

                if (body.AccountId <= 0 || body.CategoryId <= 0 || body.Amount <= 0)
                    return Results.BadRequest(new { error = "Datos inválidos" });

                // La cuenta tiene que ser del usuario y la categoría tiene que verla.
                if (await ValidateAccountAndCategory(db, userId, body.AccountId, body.CategoryId) is { } error)
                    return error;

                // Si el gasto sale de un pocket, ese pocket debe ser del usuario y de la misma cuenta.
                if (body.PocketId is int pocketId)
                {
                    var pocketOk = await db.Pockets.AnyAsync(p =>
                        p.Id == pocketId && p.UserId == userId && p.AccountId == body.AccountId);
                    if (!pocketOk)
                        return Results.BadRequest(new { error = "El pocket no existe o no es de esa cuenta" });
                }

                var date = body.ExpenseDate == default
                    ? DateTime.Now
                    : body.ExpenseDate;

                var expense = await useCase.ExecuteAsync(
                    userId,
                    body.AccountId,
                    body.CategoryId,
                    body.Amount,
                    body.Description,
                    date,
                    body.PocketId,
                    body.PetId
                );

                return Results.Created($"/expenses/{expense.Id}", expense);
        }).RequireAuthorization();

        // Gasto parcial (se paga 1 de N partes) y/o prorrateado en varias cuotas mensuales.
        // El Amount del body es el importe TOTAL: primero se divide, luego se prorratea.
        app.MapPost("/expenses/split", async (
                [FromServices] ICreateSplitSpreadExpenseUseCase useCase,
                AppDbContext db,
                [FromBody] ExpenseDto body,
                HttpContext ctx,
                [FromQuery] int? splitParts = null,
                [FromQuery] int spreadMonths = 1,
                [FromQuery] decimal? myShareAmount = null,
                [FromQuery] int? reimburseToAccountId = null,
                [FromQuery] string? debtor = null,
                [FromQuery] int? personId = null,
                // A partes iguales, quién te debe cada parte. Lista separada por comas,
                // con 0 = "sin especificar" (ej: "3,0,7"). Una por cada parte que no es tuya.
                [FromQuery] string? debtorPersonIds = null) =>
            {
                var userId = ctx.GetUserId();

                if (body.AccountId <= 0 || body.CategoryId <= 0 || body.Amount <= 0)
                    return Results.BadRequest(new { error = "Datos inválidos" });

                if (await ValidateAccountAndCategory(db, userId, body.AccountId, body.CategoryId) is { } error)
                    return error;

                // La cuenta donde te devuelven el dinero también tiene que ser tuya.
                if (reimburseToAccountId is int toAccountId)
                {
                    var toAccountOk = await db.Accounts.AnyAsync(a => a.Id == toAccountId && a.UserId == userId);
                    if (!toAccountOk)
                        return Results.NotFound(new { error = "La cuenta del reembolso no existe o no es tuya" });
                }

                // Y la persona que te lo debe tiene que estar en tu agenda.
                if (personId is int debtorPersonId)
                {
                    var personOk = await db.People.AnyAsync(x => x.Id == debtorPersonId && x.UserId == userId);
                    if (!personOk)
                        return Results.NotFound(new { error = "La persona no existe o no es tuya" });
                }

                if (body.PocketId is int pocketId)
                {
                    var pocketOk = await db.Pockets.AnyAsync(p =>
                        p.Id == pocketId && p.UserId == userId && p.AccountId == body.AccountId);
                    if (!pocketOk)
                        return Results.BadRequest(new { error = "El pocket no existe o no es de esa cuenta" });
                }

                var template = body.ToDomain();
                template.Id = 0;
                template.UserId = userId;
                template.ExpenseDate = body.ExpenseDate == default ? DateTime.Now : body.ExpenseDate;

                var (debtors, debtorNames, debtorsError) = await ResolveDebtorsAsync(db, userId, debtorPersonIds);
                if (debtorsError is not null) return debtorsError;

                var created = await useCase.ExecuteAsync(
                    template, splitParts, spreadMonths, myShareAmount, reimburseToAccountId, debtor, personId,
                    debtors, debtorNames);
                return Results.Created($"/expenses/{created[0].Id}", created);
        }).RequireAuthorization();

        // Rehacer el reparto de un gasto ya guardado: entre cuántas partes se divide,
        // quién te debe cada una, si lo adelantas tú y en cuántos meses se prorratea.
        // Mismos parámetros que el alta, y el Amount del body vuelve a ser el TOTAL.
        // Si el gasto está prorrateado se rehace el grupo entero, no solo esta cuota.
        app.MapPut("/expenses/{id:int}/split", async (
                [FromServices] IUpdateSplitSpreadExpenseUseCase useCase,
                AppDbContext db,
                [FromBody] ExpenseDto body,
                HttpContext ctx,
                int id,
                [FromQuery] int? splitParts = null,
                [FromQuery] int spreadMonths = 1,
                [FromQuery] decimal? myShareAmount = null,
                [FromQuery] int? reimburseToAccountId = null,
                [FromQuery] string? debtor = null,
                [FromQuery] int? personId = null,
                [FromQuery] string? debtorPersonIds = null) =>
            {
                var userId = ctx.GetUserId();

                if (body.AccountId <= 0 || body.CategoryId <= 0 || body.Amount <= 0)
                    return Results.BadRequest(new { error = "Datos inválidos" });

                if (await ValidateAccountAndCategory(db, userId, body.AccountId, body.CategoryId) is { } error)
                    return error;

                if (reimburseToAccountId is int toAccountId)
                {
                    var toAccountOk = await db.Accounts.AnyAsync(a => a.Id == toAccountId && a.UserId == userId);
                    if (!toAccountOk)
                        return Results.NotFound(new { error = "La cuenta del reembolso no existe o no es tuya" });
                }

                if (personId is int debtorPersonId)
                {
                    var personOk = await db.People.AnyAsync(x => x.Id == debtorPersonId && x.UserId == userId);
                    if (!personOk)
                        return Results.NotFound(new { error = "La persona no existe o no es tuya" });
                }

                if (body.PocketId is int pocketId)
                {
                    var pocketOk = await db.Pockets.AnyAsync(p =>
                        p.Id == pocketId && p.UserId == userId && p.AccountId == body.AccountId);
                    if (!pocketOk)
                        return Results.BadRequest(new { error = "El pocket no existe o no es de esa cuenta" });
                }

                var template = body.ToDomain();
                template.Id = 0;
                template.UserId = userId;
                template.ExpenseDate = body.ExpenseDate == default ? DateTime.Now : body.ExpenseDate;

                var (debtors, debtorNames, debtorsError) = await ResolveDebtorsAsync(db, userId, debtorPersonIds);
                if (debtorsError is not null) return debtorsError;

                var updated = await useCase.ExecuteAsync(
                    id, template, splitParts, spreadMonths, myShareAmount, reimburseToAccountId, debtor, personId,
                    debtors, debtorNames);
                return Results.Ok(updated);
        }).RequireAuthorization();

        app.MapDelete("/expenses/{id}",async (
            [FromServices] IDeleteExpenseUseCase useCase,
            HttpContext ctx,
            int id) =>
        {
            var deleted = await useCase.ExecuteAsync(id, ctx.GetUserId());
            return deleted
                ? Results.NoContent()
                : Results.NotFound(new { error = "Expense no existe" });
        }).RequireAuthorization();

        // Borrar de golpe todas las cuotas de un prorrateo
        app.MapDelete("/expenses/{id}/spread", async (
            [FromServices] IDeleteExpenseSpreadUseCase useCase,
            HttpContext ctx,
            int id) =>
        {
            var deleted = await useCase.ExecuteAsync(id, ctx.GetUserId());
            return Results.Ok(new { deleted });
        }).RequireAuthorization();

        app.MapGet(pattern: "/expenses/{id}", async (
            [FromServices] IGetExpenseByIdUseCase usecase,
            HttpContext ctx,
            int id) =>
        {
            var expense = await usecase.ExecuteAsync(id, ctx.GetUserId());
            if (expense is null) {
                return Results.NotFound(new { error = "Expense no existe" });
            }
            return Results.Ok(expense);

        }).RequireAuthorization();

        // Listar gastos del usuario autenticado
        app.MapGet("/expenses", async (
            [FromServices] IGetExpensesUseCase useCase,
            HttpContext ctx) =>
        {
            var expenses = await useCase.ExecuteAsync(ctx.GetUserId());
            return Results.Ok(expenses);
        }).RequireAuthorization();

        // Actualizar gasto
        app.MapPut("/expenses/{id}", async (
            [FromServices] IUpdateExpenseUseCase useCase,
            AppDbContext db,
            [FromBody] ExpenseDto body,
            HttpContext ctx,
            int id) =>
        {
            var userId = ctx.GetUserId();

            if (body.AccountId <= 0 || body.CategoryId <= 0 || body.Amount <= 0)
                return Results.BadRequest(new { error = "Datos inválidos" });

            if (await ValidateAccountAndCategory(db, userId, body.AccountId, body.CategoryId) is { } error)
                return error;

            // El endpoint construye la entidad de dominio a partir del body (CLAUDE.md)
            var expense = body.ToDomain();
            expense.Id = id;              // el id viene de la ruta
            expense.UserId = userId;      // el usuario viene del JWT
            expense.ExpenseDate = body.ExpenseDate == default
                ? DateTime.Now
                : body.ExpenseDate;

            var updated = await useCase.ExecuteAsync(expense);
            return Results.Ok(updated);
        }).RequireAuthorization();

    }

    /// <summary>
    /// Convierte la lista que llega por query ("3,0,7") en los deudores de cada parte
    /// más sus nombres: [3, null, 7]. El 0 es "no he dicho quién es".
    ///
    /// El nombre se guarda además del id para que el histórico siga diciendo quién era
    /// aunque luego borres a la persona de tu agenda. Devuelve el error a responder si
    /// alguna de las personas no es del usuario.
    /// </summary>
    private static async Task<(List<int?>? Debtors, List<string?>? Names, IResult? Error)> ResolveDebtorsAsync(
        AppDbContext db, int userId, string? debtorPersonIds)
    {
        if (string.IsNullOrWhiteSpace(debtorPersonIds))
            return (null, null, null);

        var debtors = debtorPersonIds
            .Split(',', StringSplitOptions.TrimEntries)
            .Select(x => int.TryParse(x, out var v) && v > 0 ? v : (int?)null)
            .ToList();

        // Todas las personas tienen que ser tuyas.
        var ids = debtors.Where(x => x is not null).Select(x => x!.Value).Distinct().ToList();
        if (ids.Count == 0)
            return (debtors, null, null);

        var mine = await db.People
            .Where(x => ids.Contains(x.Id) && x.UserId == userId)
            .ToDictionaryAsync(x => x.Id, x => x.Name);

        if (mine.Count != ids.Count)
            return (null, null, Results.NotFound(new { error = "Alguna persona no existe o no es tuya" }));

        var names = debtors
            .Select(x => x is int pid && mine.TryGetValue(pid, out var n) ? n : null)
            .ToList();

        return (debtors, names, null);
    }

    /// <summary>
    /// La cuenta debe ser del usuario y la categoría debe ser una que vea
    /// (común o suya). Devuelve el error a responder, o null si todo va bien.
    /// </summary>
    private static async Task<IResult?> ValidateAccountAndCategory(AppDbContext db, int userId, int accountId, int categoryId)
    {
        var accountOk = await db.Accounts.AnyAsync(a => a.Id == accountId && a.UserId == userId);
        if (!accountOk)
            return Results.NotFound(new { error = "La cuenta no existe o no es tuya" });

        var categoryOk = await db.ExpenseCategories
            .AnyAsync(c => c.Id == categoryId && (c.UserId == null || c.UserId == userId));
        if (!categoryOk)
            return Results.NotFound(new { error = "La categoría no existe o no es tuya" });

        return null;
    }
}
