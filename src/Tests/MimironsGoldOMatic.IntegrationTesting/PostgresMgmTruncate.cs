using Npgsql;

namespace MimironsGoldOMatic.IntegrationTesting;

/// <summary>Clears all tables in the Marten <c>mgm</c> schema between tests (Docker PostgreSQL).</summary>
public static class PostgresMgmTruncate
{
    private const string Sql =
        """
        DO $$
        DECLARE r RECORD;
        BEGIN
          FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'mgm')
          LOOP
            EXECUTE format('TRUNCATE TABLE mgm.%I CASCADE', r.tablename);
          END LOOP;
        END $$;
        """;

    public static async Task TruncateAllAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(Sql, conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
