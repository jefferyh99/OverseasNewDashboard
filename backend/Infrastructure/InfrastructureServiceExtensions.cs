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

        services.AddDbContext<OpsMonitorDbContext>(opts =>
        {
            if (provider == "Sqlite")
            {
                var connStr = configuration.GetConnectionString("DefaultConnection")
                    ?? "Data Source=ops-monitor-dev.db";
                opts.UseSqlite(connStr);
            }
            else
            {
                var connStr = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required for SqlServer.");
                opts.UseSqlServer(connStr);
            }
        });

        services.AddSingleton<BusinessReadConnectionFactory>();

        return services;
    }
}
