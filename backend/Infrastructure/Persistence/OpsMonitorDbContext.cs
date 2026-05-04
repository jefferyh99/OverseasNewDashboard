using Microsoft.EntityFrameworkCore;
using OpsMonitor.Domain;

namespace OpsMonitor.Infrastructure.Persistence;

public class OpsMonitorDbContext(DbContextOptions<OpsMonitorDbContext> options) : DbContext(options)
{
    public DbSet<ReminderLog> ReminderLogs => Set<ReminderLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReminderLog>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.ObjectType).HasMaxLength(20).IsRequired();
            e.Property(r => r.ObjectId).HasMaxLength(100).IsRequired();
            e.Property(r => r.EventType).HasMaxLength(20).IsRequired();
            e.Property(r => r.Channel).HasMaxLength(20).IsRequired();
            e.HasIndex(r => new { r.ObjectType, r.ObjectId, r.EventType, r.Channel });
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpsMonitorDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
