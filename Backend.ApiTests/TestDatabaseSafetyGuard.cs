using Microsoft.Data.SqlClient;

namespace Backend.ApiTests;

/// <summary>
/// Fail-closed guard for tests that can mutate a database. It never falls back
/// to an application connection string and verifies the server's actual DB name.
/// </summary>
public static class TestDatabaseSafetyGuard
{
    public const string UnsafeDatabaseMessage =
        "Unsafe test database detected. Mutating tests may only run against LMS_TEST_*.";

    public static string GetVerifiedTestConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable("LMS_TEST_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(UnsafeDatabaseMessage);

        VerifyActualDatabaseName(connectionString);
        return connectionString;
    }

    public static void VerifyActualDatabaseName(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(UnsafeDatabaseMessage);

        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = new SqlCommand("SELECT DB_NAME();", connection);
        var databaseName = command.ExecuteScalar() as string;
        EnsureAllowedDatabaseName(databaseName);
    }

    internal static void EnsureAllowedDatabaseName(string? databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName) ||
            !databaseName.StartsWith("LMS_TEST_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(UnsafeDatabaseMessage);
        }
    }
}
