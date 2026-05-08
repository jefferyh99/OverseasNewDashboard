using OpsMonitor.Infrastructure;
using OpsMonitor.Api.Middleware;
using OpsMonitor.Api.Auth;
using OpsMonitor.Api.BackgroundServices;
using OpsMonitor.Application.Outbound;
using OpsMonitor.Application.Settings;
using OpsMonitor.Application.Notifications;
using OpsMonitor.Infrastructure.Notifications;
using OpsMonitor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<AlertsSettingsStore>();
builder.Services.AddScoped<WarehouseOutboundRuleProvider>();
builder.Services.AddScoped<WarehouseBusinessCalendarService>();
builder.Services.AddScoped<OutboundDeadlineCalculator>();

// Application services
builder.Services.AddScoped<IAnomalyDataSource, MockAnomalyDataSource>();
builder.Services.AddScoped<ReminderDispatchService>();
builder.Services.AddHostedService<ReminderScanHostedService>();

// Authentication — mock bearer for scaffold; swap for real JWT in production
builder.Services
    .AddAuthentication(MockBearerAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, MockBearerAuthenticationHandler>(
        MockBearerAuthenticationHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();

// EF Core: auto-create tables (dev only; use migrations in production)
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<OpsMonitorDbContext>().Database.EnsureCreated();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();

app.Run();

// Needed for WebApplicationFactory in tests
public partial class Program { }


