using Microsoft.EntityFrameworkCore;

namespace OpsMonitor.Infrastructure.Persistence;

public class OpsMonitorDbContext(DbContextOptions<OpsMonitorDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpsMonitorDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
