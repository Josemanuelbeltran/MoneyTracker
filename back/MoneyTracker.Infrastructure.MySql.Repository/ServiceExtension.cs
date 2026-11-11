using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.Domain.Repository;
using MoneyTracker.Infrastructure.MySql.Repository.Account;
using MoneyTracker.Infrastructure.MySql.Repository.AccountType;
using MoneyTracker.Infrastructure.MySql.Repository.Expense;
using MoneyTracker.Infrastructure.MySql.Repository.ExpenseReimbursement;
using MoneyTracker.Infrastructure.MySql.Repository.ExpenseCategory;
using MoneyTracker.Infrastructure.MySql.Repository.Income;
using MoneyTracker.Infrastructure.MySql.Repository.IncomeCategory;
using MoneyTracker.Infrastructure.MySql.Repository.CategoryBudget;
using MoneyTracker.Infrastructure.MySql.Repository.Person;
using MoneyTracker.Infrastructure.MySql.Repository.Pet;
using MoneyTracker.Infrastructure.MySql.Repository.Pocket;
using MoneyTracker.Infrastructure.MySql.Repository.Transfer;
using MoneyTracker.Infrastructure.MySql.Repository.User;

namespace MoneyTracker.Infrastructure.MySql.Repository;

public static class ServiceExtension
{
    public static IServiceCollection AddRepositoryService(this IServiceCollection services)
    {
        services
            .AddScoped<IExpenseRepository, ExpenseRepository>()
            .AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>()
            .AddScoped<IIncomeRepository, IncomeRepository>()
            .AddScoped<IIncomeCategoryRepository, IncomeCategoryRepository>()
            .AddScoped<ITransferRepository, TransferRepository>()
            .AddScoped<IExpenseReimbursementRepository, ExpenseReimbursementRepository>()
            .AddScoped<IPersonRepository, PersonRepository>()
            .AddScoped<IPetRepository, PetRepository>()
            .AddScoped<IPocketRepository, PocketRepository>()
            .AddScoped<ICategoryBudgetRepository, CategoryBudgetRepository>()
            .AddScoped<IAccountRepository, AccountRepository>()
            .AddScoped<IAccountTypeRepository, AccountTypeRepository>()
            .AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
    
}