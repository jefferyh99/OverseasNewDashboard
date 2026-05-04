using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpsMonitor.Infrastructure.Persistence;

namespace OpsMonitor.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Persistence:Provider"] ?? "SqlServer";

        // EF Core 10 enforces single-provider-per-app: having both SqlServer and Sqlite
        // packages in the same project causes duplicate service registration.
        // Base template ships with Sqlite only. For SQL Server production support:
        //   1. dotnet add package Microsoft.EntityFrameworkCore.SqlServer
        //   2. Replace the throw below with: opts.UseSqlServer(connStr)
        if (provider == "Sqlite")
        {
            var connStr = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=ops-monitor-dev.db";
            services.AddDbContext<OpsMonitorDbContext>(opts => opts.UseSqlite(connStr));
        }
        else
        {
            throw new NotSupportedException(
                "SQL Server provider is not installed in the base template. " +
                "Run: dotnet add Infrastructure package Microsoft.EntityFrameworkCore.SqlServer " +
                "and update InfrastructureServiceExtensions to call opts.UseSqlServer(connStr).");
        }

        services.AddSingleton<BusinessReadConnectionFactory>();

        return services;
    }
}

