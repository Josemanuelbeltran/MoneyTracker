using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.Domain.QueryService;
using MoneyTracker.Infrastructure.MySql.QueryService.Account;
using MoneyTracker.Infrastructure.MySql.QueryService.Expense;
using MoneyTracker.Infrastructure.MySql.QueryService.ExpenseReimbursement;
using MoneyTracker.Infrastructure.MySql.QueryService.ExpenseCategory;
using MoneyTracker.Infrastructure.MySql.QueryService.AccountType;
using MoneyTracker.Infrastructure.MySql.QueryService.Income;
using MoneyTracker.Infrastructure.MySql.QueryService.IncomeCategory;
using MoneyTracker.Infrastructure.MySql.QueryService.CategoryBudget;
using MoneyTracker.Infrastructure.MySql.QueryService.Person;
using MoneyTracker.Infrastructure.MySql.QueryService.Pet;
using MoneyTracker.Infrastructure.MySql.QueryService.Pocket;
using MoneyTracker.Infrastructure.MySql.QueryService.Transfer;
using MoneyTracker.Infrastructure.MySql.QueryService.User;

namespace MoneyTracker.Infrastructure.MySql.QueryService;

public static class ServiceExtension
{
    public static IServiceCollection AddQueryService(this IServiceCollection services)
    {
        
        services
            .AddScoped<IExpenseQueryService, ExpenseQueryService>()
            .AddScoped<IExpenseCategoryQueryService, ExpenseCategoryQueryService>()
            .AddScoped<IIncomeQueryService, IncomeQueryService>()
            .AddScoped<IIncomeCategoryQueryService, IncomeCategoryQueryService>()
            .AddScoped<ITransferQueryService, TransferQueryService>()
            .AddScoped<IExpenseReimbursementQueryService, ExpenseReimbursementQueryService>()
            .AddScoped<IPersonQueryService, PersonQueryService>()
            .AddScoped<IPetQueryService, PetQueryService>()
            .AddScoped<IPocketQueryService, PocketQueryService>()
            .AddScoped<ICategoryBudgetQueryService, CategoryBudgetQueryService>()
            .AddScoped<IAccountTypeQueryService, AccountTypeQueryService>()
            .AddScoped<IAccountQueryService, AccountQueryService>()
            .AddScoped<IUserQueryService, UserQueryService>();
        return services;
    }
}