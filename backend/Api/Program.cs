using OpsMonitor.Infrastructure;
using OpsMonitor.Api.Middleware;
using OpsMonitor.Api.Auth;
using Microsoft.AspNetCore.Authentication;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);

// Authentication — mock bearer for scaffold; swap for real JWT in production
builder.Services
    .AddAuthentication(MockBearerAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, MockBearerAuthenticationHandler>(
        MockBearerAuthenticationHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();

app.Run();

// Needed for WebApplicationFactory in tests
public partial class Program { }


