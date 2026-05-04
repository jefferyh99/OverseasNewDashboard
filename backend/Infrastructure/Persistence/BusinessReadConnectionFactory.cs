using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace OpsMonitor.Infrastructure.Persistence;

/// <summary>
/// Factory that creates read-only IDbConnection for Dapper queries.
/// Selects the provider based on Persistence:Provider configuration ("SqlServer" | "Sqlite").
/// </summary>
public class BusinessReadConnectionFactory(IConfiguration configuration)
{
    private readonly string _provider = configuration["Persistence:Provider"] ?? "SqlServer";
    private readonly string _connectionString = configuration.GetConnectionString("BusinessRead")
        ?? throw new InvalidOperationException("ConnectionStrings:BusinessRead is not configured.");

    public IDbConnection Create() => _provider switch
    {
        "Sqlite" => new SqliteConnection(_connectionString),
        _ => new SqlConnection(_connectionString)
    };
}
