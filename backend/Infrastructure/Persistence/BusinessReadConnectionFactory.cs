using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace OpsMonitor.Infrastructure.Persistence;

/// <summary>
/// Factory that creates read-only IDbConnection for Dapper queries.
/// Base template uses Sqlite. For SQL Server: install Microsoft.Data.SqlClient,
/// add the using, and replace the default branch below with new SqlConnection(_connectionString).
/// </summary>
public class BusinessReadConnectionFactory(IConfiguration configuration)
{
    private readonly string _provider = configuration["Persistence:Provider"] ?? "SqlServer";
    private readonly string _connectionString = configuration.GetConnectionString("BusinessRead")
        ?? throw new InvalidOperationException("ConnectionStrings:BusinessRead is not configured.");

    public IDbConnection Create() => _provider switch
    {
        "Sqlite" => new SqliteConnection(_connectionString),
        _ => throw new NotSupportedException(
            "SQL Server provider not installed. Add Microsoft.Data.SqlClient package and update this factory.")
    };
}

