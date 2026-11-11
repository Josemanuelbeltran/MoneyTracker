using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.Infrastructure.MySql.DDBBConection;

namespace MoneyTracker.Infrastructure.MySql;

public static class ServiceExtension
{
    public static IServiceCollection AddMySqlPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en appsettings.json");

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(cs, ServerVersion.AutoDetect(cs)));

        return services;
    }
}