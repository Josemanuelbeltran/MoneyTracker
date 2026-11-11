using Microsoft.EntityFrameworkCore;
using MoneyTracker.Infrastructure.MySql.Dtos;

namespace MoneyTracker.Infrastructure.MySql.DDBBConection;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<UserDto> Users => Set<UserDto>();
    public DbSet<AccountTypeDto> AccountTypes => Set<AccountTypeDto>();
    public DbSet<AccountDto> Accounts => Set<AccountDto>();
    public DbSet<ExpenseCategoryDto> ExpenseCategories => Set<ExpenseCategoryDto>();
    public DbSet<IncomeCategoryDto> IncomeCategories => Set<IncomeCategoryDto>();
    public DbSet<ExpenseDto> Expenses => Set<ExpenseDto>();
    public DbSet<IncomeDto> Incomes => Set<IncomeDto>();
    public DbSet<TransferDto> Transfers => Set<TransferDto>();
    public DbSet<PocketDto> Pockets => Set<PocketDto>();
    public DbSet<CategoryBudgetDto> CategoryBudgets => Set<CategoryBudgetDto>();
    public DbSet<BudgetDto> Budgets => Set<BudgetDto>();
    public DbSet<RealSpentMonthlyDto> RealSpentMonthly => Set<RealSpentMonthlyDto>();
    public DbSet<ExpenseReimbursementDto> ExpenseReimbursements => Set<ExpenseReimbursementDto>();
    public DbSet<PersonDto> People => Set<PersonDto>();
    public DbSet<PetDto> Pets => Set<PetDto>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dateOnlyConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateOnly, DateTime>(
            v => v.ToDateTime(TimeOnly.MinValue),
            v => DateOnly.FromDateTime(v));

        modelBuilder.Entity<UserDto>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("int unsigned");

            e.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            e.Property(x => x.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            e.Property(x => x.PhoneNumber)
                .HasColumnName("phone_number")
                .HasMaxLength(15);

            e.Property(x => x.VerifiedPhone)
                .HasColumnName("verified_phone")
                .IsRequired();

            // Administrador: decide qué categorías son comunes para todos.
            e.Property(x => x.IsAdmin)
                .HasColumnName("is_admin")
                .IsRequired();

            e.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            e.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();
        });


        modelBuilder.Entity<AccountTypeDto>(e =>
        {
            e.ToTable("AccountTypes");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("int unsigned");

            e.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            e.HasIndex(x => x.Name).IsUnique();
        });


        modelBuilder.Entity<AccountDto>(e =>
        {
            e.ToTable("Accounts");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("int unsigned");

            e.Property(x => x.UserId)
                .HasColumnName("user_id")
                .HasColumnType("int unsigned")
                .IsRequired();

            e.Property(x => x.AccountTypeId)
                .HasColumnName("account_type_id")
                .HasColumnType("int unsigned")
                .IsRequired();

            e.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            e.Property(x => x.Balance)
                .HasColumnName("balance")
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0m);

            e.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            e.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            // FK -> Users (sin propiedades de navegación)
            e.HasOne<UserDto>()
                .WithMany()                        // sin colección en el principal
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // FK -> AccountTypes (sin propiedades de navegación)
            e.HasOne<AccountTypeDto>()
                .WithMany()
                .HasForeignKey(x => x.AccountTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.UserId).HasDatabaseName("idx_accounts_user");
        });


        modelBuilder.Entity<ExpenseCategoryDto>(e =>
        {
            e.ToTable("ExpenseCategories");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id").HasColumnType("int unsigned");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(x => x.ParentId).HasColumnName("parent_id").HasColumnType("int unsigned");
            // NULL = categoría común del sistema; con valor = propia de ese usuario.
            e.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("int unsigned");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            // self-FK sin navegaciones
            e.HasOne<ExpenseCategoryDto>()
                .WithMany()
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<UserDto>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.UserId).HasDatabaseName("ix_expcat_user");
        });

        modelBuilder.Entity<IncomeCategoryDto>(e =>
        {
            e.ToTable("IncomeCategories");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id").HasColumnType("int unsigned");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(x => x.ParentId).HasColumnName("parent_id").HasColumnType("int unsigned");
            // NULL = categoría común del sistema; con valor = propia de ese usuario.
            e.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("int unsigned");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            e.HasOne<IncomeCategoryDto>()
                .WithMany()
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<UserDto>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.UserId).HasDatabaseName("ix_inccat_user");
        });

        modelBuilder.Entity<ExpenseDto>(e =>
        {
            e.ToTable("Expenses");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id").HasColumnType("int unsigned");
            e.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("int unsigned");
            e.Property(x => x.AccountId).HasColumnName("account_id").HasColumnType("int unsigned");
            e.Property(x => x.CategoryId).HasColumnName("category_id").HasColumnType("int unsigned");
            e.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
            e.Property(x => x.ExpenseDate).HasColumnName("expense_date");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.IsRecurring).HasColumnName("is_recurring");
            e.Property(x => x.PocketId).HasColumnName("pocket_id").HasColumnType("int unsigned");
            // Mascota del gasto: el id es el de PetRecording, sin FK a Pets a proposito.
            e.Property(x => x.PetId).HasColumnName("pet_id").HasColumnType("int unsigned");
            e.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(10,2)");
            e.Property(x => x.SplitParts).HasColumnName("split_parts").HasColumnType("int unsigned");
            // varchar(36) a proposito, NO char(36): MySqlConnector lee por defecto toda
            // columna CHAR(36) como System.Guid y aqui la propiedad es un string.
            e.Property(x => x.SpreadGroup).HasColumnName("spread_group").HasColumnType("varchar(36)");
            e.Property(x => x.SpreadIndex).HasColumnName("spread_index").HasColumnType("int unsigned");
            e.Property(x => x.SpreadCount).HasColumnName("spread_count").HasColumnType("int unsigned");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            e.HasOne<UserDto>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<AccountDto>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<ExpenseCategoryDto>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PocketDto>().WithMany().HasForeignKey(x => x.PocketId).OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => new { x.UserId, x.ExpenseDate }).HasDatabaseName("idx_exp_user_date");
            e.HasIndex(x => new { x.AccountId, x.ExpenseDate }).HasDatabaseName("idx_exp_account_date");
            e.HasIndex(x => new { x.CategoryId, x.ExpenseDate }).HasDatabaseName("idx_exp_category_date");
        });

        modelBuilder.Entity<IncomeDto>(e =>
        {
            e.ToTable("Incomes");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id").HasColumnType("int unsigned");
            e.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("int unsigned");
            e.Property(x => x.AccountId).HasColumnName("account_id").HasColumnType("int unsigned");
            e.Property(x => x.CategoryId).HasColumnName("category_id").HasColumnType("int unsigned");
            e.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
            e.Property(x => x.IncomeDate).HasColumnName("income_date").HasConversion(dateOnlyConverter).HasColumnType("date");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.IsRecurring).HasColumnName("is_recurring");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            e.HasOne<UserDto>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<AccountDto>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<IncomeCategoryDto>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.UserId, x.IncomeDate }).HasDatabaseName("idx_inc_user_date");
            e.HasIndex(x => new { x.AccountId, x.IncomeDate }).HasDatabaseName("idx_inc_account_date");
            e.HasIndex(x => new { x.CategoryId, x.IncomeDate }).HasDatabaseName("idx_inc_category_date");
        });


        modelBuilder.Entity<TransferDto>(e =>
        {
            e.ToTable("Transfers");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id").HasColumnType("int unsigned");
            e.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("int unsigned");
            e.Property(x => x.FromAccountId).HasColumnName("from_account_id").HasColumnType("int unsigned");
            e.Property(x => x.ToAccountId).HasColumnName("to_account_id").HasColumnType("int unsigned");
            e.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
            e.Property(x => x.TransferDate).HasColumnName("transfer_date");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            e.HasOne<UserDto>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<AccountDto>().WithMany().HasForeignKey(x => x.FromAccountId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<AccountDto>().WithMany().HasForeignKey(x => x.ToAccountId).OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.UserId, x.TransferDate }).HasDatabaseName("idx_tr_user_date");
        });

        // Parte de un gasto compartido que el usuario adelantó y le tienen que devolver.
        modelBuilder.Entity<ExpenseReimbursementDto>(e =>
        {
            e.ToTable("ExpenseReimbursements");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id").HasColumnType("int unsigned");
            e.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("int unsigned");
            e.Property(x => x.ExpenseId).HasColumnName("expense_id").HasColumnType("int unsigned");
            e.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
            e.Property(x => x.ToAccountId).HasColumnName("to_account_id").HasColumnType("int unsigned");
            e.Property(x => x.SettledAt).HasColumnName("settled_at");
            e.Property(x => x.Debtor).HasColumnName("debtor").HasMaxLength(100);
            e.Property(x => x.PersonId).HasColumnName("person_id").HasColumnType("int unsigned");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            e.HasOne<UserDto>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<ExpenseDto>().WithMany().HasForeignKey(x => x.ExpenseId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<AccountDto>().WithMany().HasForeignKey(x => x.ToAccountId).OnDelete(DeleteBehavior.Restrict);
            // Si borras a la persona el reembolso se queda, con el nombre en `debtor`.
            e.HasOne<PersonDto>().WithMany().HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => new { x.UserId, x.SettledAt }).HasDatabaseName("ix_expreimb_pending");
        });

        // Agenda de personas con las que compartes gastos.
        modelBuilder.Entity<PersonDto>(e =>
        {
            e.ToTable("People");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id").HasColumnType("int unsigned");
            e.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("int unsigned");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            e.HasOne<UserDto>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.UserId, x.Name }).IsUnique().HasDatabaseName("uq_people_user_name");
        });

        // Espejo de las mascotas de PetRecording: el id es el suyo (a mano, sin
        // autoincremental) y el nombre solo sirve para el desplegable del gasto.
        modelBuilder.Entity<PetDto>(e =>
        {
            e.ToTable("Pets");
            // La clave es (usuario, id): el id es el de PetRecording y es unico
            // dentro de cada usuario, no entre todos.
            e.HasKey(x => new { x.UserId, x.Id });

            e.Property(x => x.Id).HasColumnName("id").HasColumnType("int unsigned").ValueGeneratedNever();
            e.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("int unsigned");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();

            e.HasOne<UserDto>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PocketDto>(e =>
        {
            e.ToTable("Pockets");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id").HasColumnType("int unsigned");
            e.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("int unsigned");
            e.Property(x => x.AccountId).HasColumnName("account_id").HasColumnType("int unsigned");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(x => x.Balance).HasColumnName("balance").HasColumnType("decimal(10,2)").HasDefaultValue(0m);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            e.HasOne<UserDto>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<AccountDto>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.AccountId).HasDatabaseName("idx_pocket_account");
        });

        modelBuilder.Entity<CategoryBudgetDto>(e =>
        {
            e.ToTable("CategoryBudgets");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id").HasColumnType("int unsigned");
            e.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("int unsigned");
            e.Property(x => x.CategoryId).HasColumnName("category_id").HasColumnType("int unsigned");
            e.Property(x => x.MonthlyLimit).HasColumnName("monthly_limit").HasColumnType("decimal(10,2)");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            e.HasOne<UserDto>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<ExpenseCategoryDto>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.UserId, x.CategoryId }).IsUnique().HasDatabaseName("uq_catbudget_user_cat");
        });

        modelBuilder.Entity<BudgetDto>(e =>
        {
            e.ToTable("Budgets");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id").HasColumnType("int unsigned");
            e.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("int unsigned");
            e.Property(x => x.CategoryId).HasColumnName("category_id").HasColumnType("int unsigned");
            e.Property(x => x.AmountLimit).HasColumnName("amount_limit").HasColumnType("decimal(10,2)");
            e.Property(x => x.PeriodStart).HasColumnName("period_start").HasConversion(dateOnlyConverter).HasColumnType("date");
            e.Property(x => x.PeriodEnd).HasColumnName("period_end").HasConversion(dateOnlyConverter).HasColumnType("date");
            e.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(255);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            e.HasOne<UserDto>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<ExpenseCategoryDto>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.UserId, x.PeriodStart, x.PeriodEnd }).HasDatabaseName("idx_budget_user_period");
            e.HasIndex(x => new { x.CategoryId, x.PeriodStart, x.PeriodEnd }).HasDatabaseName("idx_budget_category_period");
            e.HasIndex(x => new { x.UserId, x.CategoryId, x.PeriodStart, x.PeriodEnd }).IsUnique().HasDatabaseName("uq_budget_user_cat_period");
        });


        modelBuilder.Entity<RealSpentMonthlyDto>(e =>
        {
            e.HasNoKey();
            e.ToView("RealSpentMonthly");

            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.CategoryId).HasColumnName("category_id");
            e.Property(x => x.Year).HasColumnName("y");
            e.Property(x => x.Month).HasColumnName("m");
            e.Property(x => x.SpentAmount).HasColumnName("spent_amount").HasColumnType("decimal(10,2)");
        });

            }
}

