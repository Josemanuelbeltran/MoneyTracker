using ClassLibrary1MoneyTracker.Application.Security;
using ClassLibrary1MoneyTracker.Application.UseCases.Account;
using ClassLibrary1MoneyTracker.Application.UseCases.AccountType;
using ClassLibrary1MoneyTracker.Application.UseCases.Expense;
using ClassLibrary1MoneyTracker.Application.UseCases.ExpenseCategories;
using ClassLibrary1MoneyTracker.Application.UseCases.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.Application.UseCases.Account;
using MoneyTracker.Application.UseCases.Expense;
using MoneyTracker.Application.UseCases.ExpenseCategories;
using MoneyTracker.Application.UseCases.Income;
using MoneyTracker.Application.UseCases.IncomeCategories;
using MoneyTracker.Application.UseCases.Recurring;
using MoneyTracker.Application.UseCases.CategoryBudget;
using MoneyTracker.Application.UseCases.Pocket;
using MoneyTracker.Application.UseCases.Transfer;
using MoneyTracker.Application.UseCases.ExpenseReimbursement;
using MoneyTracker.Application.UseCases.Person;
using MoneyTracker.Application.UseCases.Pet;
using MoneyTracker.Application.Integration;
using MoneyTracker.Domain.Application.Account;
using MoneyTracker.Domain.Application.AccountType;
using MoneyTracker.Domain.Application.Expense;
using MoneyTracker.Domain.Application.ExpenseCategory;
using MoneyTracker.Domain.Application.Income;
using MoneyTracker.Domain.Application.IncomeCategory;
using MoneyTracker.Domain.Application.Recurring;
using MoneyTracker.Domain.Application.CategoryBudget;
using MoneyTracker.Domain.Application.Pocket;
using MoneyTracker.Domain.Application.Transfer;
using MoneyTracker.Domain.Application.ExpenseReimbursement;
using MoneyTracker.Domain.Application.Person;
using MoneyTracker.Domain.Application.Pet;
using MoneyTracker.Domain.Integration;
using MoneyTracker.Domain.Application.Security;
using MoneyTracker.Domain.Application.User;

namespace ClassLibrary1MoneyTracker.Application;

public static class ServiceExtension
{
    public static IServiceCollection AddApplicationService(this IServiceCollection services, IConfiguration configuration)
    {
        //User
        services
            .AddScoped<ILoginUseCase, LoginUseCase>()
            .AddScoped<IGetUserByEmailUseCase, GetUserByEmail>()
            .AddScoped<IGetUserByNameUseCase, GetUserByNameUseCase>()
            .AddScoped<IRegisterUseCase, RegisterUseCase>()
            .AddScoped<IAddUserUseCase, AddUserUseCase>();
        
        //AccountType
        services
            .AddScoped<IGetAccountTypesUseCase, GetAccountTypesUseCase>()
            .AddScoped<IUpdateAccountTypeUseCase, UpdateAccountTypeUseCase>()
            .AddScoped<IDeleteAccountTypeByIdUseCase, DeleteAccountTypeByIdUseCase>()
            .AddScoped<ICreateAccountTypeUseCase, CreateAccountTypeUseCase>();
        
        //Account
        services
            .AddScoped<IGetAccountsUseCase, GetAccountsUseCase>()
            .AddScoped<IUpdateAccountUseCase, UpdateAccountUseCase>()
            .AddScoped<IGetAccountByIdUseCase, GetAccountByIdUseCase>()
            .AddScoped<IDeleteAccountUseCase, DeleteAccountUseCase>()
            .AddScoped<ICreateAccountUseCase, CreateAccountUseCase>();
        
        //ExpenseCategory
        services
            .AddScoped<IGetAllExpenseCategoriesUseCase, GetAllExpenseCategoriesUseCase>()
            .AddScoped<IUpdateExpenseCategoryUseCase, UpdateExpenseCategoryUseCase>()
            .AddScoped<IDeleteExpenseCategoryUseCase, DeleteExpenseCategoryUseCase>()
            .AddScoped<IGetExpenseCategoryByIdUseCase, GetExpenseCategoryByIdUseCase>()
            .AddScoped<ICreateExpenseCategoryUseCase, CreateExpenseCategoryUseCase>();
        
        //Expense
        services
            .AddScoped<IGetExpensesUseCase, GetExpensesUseCase>()
            .AddScoped<IUpdateExpenseUseCase, UpdateExpenseUseCase>()
            .AddScoped<IDeleteExpenseUseCase, DeleteExpenseUseCase>()
            .AddScoped<IGetExpenseByIdUseCase, GetExpenseByIdUseCase>()
            .AddScoped<ICreateExpenseUseCase, CreateExpenseUseCase>()
            .AddScoped<ICreateSplitSpreadExpenseUseCase, CreateSplitSpreadExpenseUseCase>()
            .AddScoped<IUpdateSplitSpreadExpenseUseCase, UpdateSplitSpreadExpenseUseCase>()
            .AddScoped<IDeleteExpenseSpreadUseCase, DeleteExpenseSpreadUseCase>();

        //Income
        services
            .AddScoped<IGetIncomesUseCase, GetIncomesUseCase>()
            .AddScoped<IUpdateIncomeUseCase, UpdateIncomeUseCase>()
            .AddScoped<IDeleteIncomeUseCase, DeleteIncomeUseCase>()
            .AddScoped<IGetIncomeByIdUseCase, GetIncomeByIdUseCase>()
            .AddScoped<ICreateIncomeUseCase, CreateIncomeUseCase>()
            .AddScoped<ICreateRecurringIncomeUseCase, CreateRecurringIncomeUseCase>()
            .AddScoped<IUpdateIncomeSeriesUseCase, UpdateIncomeSeriesUseCase>()
            .AddScoped<IDeleteIncomeSeriesUseCase, DeleteIncomeSeriesUseCase>();

        //IncomeCategory
        services
            .AddScoped<IGetIncomeCategoriesUseCase, GetIncomeCategoriesUseCase>()
            .AddScoped<ICreateIncomeCategoryUseCase, CreateIncomeCategoryUseCase>()
            .AddScoped<IUpdateIncomeCategoryUseCase, UpdateIncomeCategoryUseCase>()
            .AddScoped<IDeleteIncomeCategoryUseCase, DeleteIncomeCategoryUseCase>();

        //Transfer
        services
            .AddScoped<IGetTransfersUseCase, GetTransfersUseCase>()
            .AddScoped<ICreateTransferUseCase, CreateTransferUseCase>()
            .AddScoped<IDeleteTransferUseCase, DeleteTransferUseCase>()
            // Reembolsos de gastos compartidos (lo que adelantaste y te devuelven)
            .AddScoped<IGetReimbursementsUseCase, GetReimbursementsUseCase>()
            .AddScoped<ISettleReimbursementUseCase, SettleReimbursementUseCase>()
            .AddScoped<IUpdateReimbursementUseCase, UpdateReimbursementUseCase>()
            .AddScoped<IDeleteReimbursementUseCase, DeleteReimbursementUseCase>()
            // Agenda de personas con las que compartes gastos
            .AddScoped<IGetPeopleUseCase, GetPeopleUseCase>()
            .AddScoped<ICreatePersonUseCase, CreatePersonUseCase>()
            .AddScoped<IUpdatePersonUseCase, UpdatePersonUseCase>()
            .AddScoped<IDeletePersonUseCase, DeletePersonUseCase>();

        //CategoryBudget
        services
            .AddScoped<IGetBudgetsUseCase, GetBudgetsUseCase>()
            .AddScoped<IUpsertBudgetUseCase, UpsertBudgetUseCase>()
            .AddScoped<IDeleteBudgetUseCase, DeleteBudgetUseCase>();

        //Pocket
        services
            .AddScoped<IGetPocketsUseCase, GetPocketsUseCase>()
            .AddScoped<ICreatePocketUseCase, CreatePocketUseCase>()
            .AddScoped<IUpdatePocketUseCase, UpdatePocketUseCase>()
            .AddScoped<IDeletePocketUseCase, DeletePocketUseCase>()
            .AddScoped<IMovePocketUseCase, MovePocketUseCase>();

        //Pet (gastos de mascota que se mandan a PetRecording)
        services
            .AddScoped<IGetPetsUseCase, GetPetsUseCase>()
            .AddScoped<ICreatePetUseCase, CreatePetUseCase>()
            .AddScoped<IPetExpenseNotifier, PetExpenseNotifier>();

        // La otra app esta en la misma maquina: si no contesta rapido, no vamos a
        // tener al usuario esperando para apuntar un gasto.
        services.AddHttpClient<IPetRecordingClient, PetRecordingClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        //Recurring
        services
            .AddScoped<IGenerateRecurringUseCase, GenerateRecurringUseCase>();

        //Security
        services
            .AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        
        return services;
    }
}