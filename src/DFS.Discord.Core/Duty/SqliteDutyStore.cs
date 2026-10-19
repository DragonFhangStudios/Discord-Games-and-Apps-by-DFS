using Microsoft.Data.Sqlite;

namespace DFS.Discord.Core.Duty;

/// <summary>
/// SQLite-backed duty storage. Discord IDs are stored as signed 64-bit integers
/// and timestamps as Unix milliseconds (UTC).
/// </summary>
public sealed class SqliteDutyStore : IDutyStore
{
    private const string Columns = "id, guild_id, user_id, started_at, ended_at, note";

    private readonly string _connectionString;

    public SqliteDutyStore(string databasePath)
    {
        _connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();
        EnsureSchema();
    }

    public DutySession? GetOpenSession(ulong guildId, ulong userId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT {Columns} FROM duty_sessions
            WHERE guild_id = $guild AND user_id = $user AND ended_at IS NULL
            """;
        command.Parameters.AddWithValue("$guild", (long)guildId);
        command.Parameters.AddWithValue("$user", (long)userId);
        return ReadAll(command).SingleOrDefault();
    }

    public DutySession? TryStartSession(ulong guildId, ulong userId, DateTimeOffset startedAt, string? note)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        // The partial unique index turns a second open session into a no-op instead of a duplicate.
        command.CommandText = $"""
            INSERT OR IGNORE INTO duty_sessions (guild_id, user_id, started_at, note)
            VALUES ($guild, $user, $started, $note)
            RETURNING {Columns}
            """;
        command.Parameters.AddWithValue("$guild", (long)guildId);
        command.Parameters.AddWithValue("$user", (long)userId);
        command.Parameters.AddWithValue("$started", startedAt.ToUnixTimeMilliseconds());
        command.Parameters.AddWithValue("$note", (object?)note ?? DBNull.Value);
        return ReadAll(command).SingleOrDefault();
    }

    public DutySession? TryEndSession(long sessionId, DateTimeOffset endedAt)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            UPDATE duty_sessions SET ended_at = $ended
            WHERE id = $id AND ended_at IS NULL
            RETURNING {Columns}
            """;
        command.Parameters.AddWithValue("$id", sessionId);
        command.Parameters.AddWithValue("$ended", endedAt.ToUnixTimeMilliseconds());
        return ReadAll(command).SingleOrDefault();
    }

    public IReadOnlyList<DutySession> GetOpenSessions(ulong guildId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT {Columns} FROM duty_sessions
            WHERE guild_id = $guild AND ended_at IS NULL
            ORDER BY started_at
            """;
        command.Parameters.AddWithValue("$guild", (long)guildId);
        return ReadAll(command);
    }

    public IReadOnlyList<DutySession> GetSessionsOverlapping(ulong guildId, DateTimeOffset from, DateTimeOffset to)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT {Columns} FROM duty_sessions
            WHERE guild_id = $guild
              AND started_at < $to
              AND (ended_at IS NULL OR ended_at > $from)
            ORDER BY started_at
            """;
        command.Parameters.AddWithValue("$guild", (long)guildId);
        command.Parameters.AddWithValue("$from", from.ToUnixTimeMilliseconds());
        command.Parameters.AddWithValue("$to", to.ToUnixTimeMilliseconds());
        return ReadAll(command);
    }

    private void EnsureSchema()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode = WAL;

            CREATE TABLE IF NOT EXISTS duty_sessions (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                guild_id    INTEGER NOT NULL,
                user_id     INTEGER NOT NULL,
                started_at  INTEGER NOT NULL,
                ended_at    INTEGER NULL,
                note        TEXT NULL
            );

            CREATE UNIQUE INDEX IF NOT EXISTS ux_duty_sessions_one_open
                ON duty_sessions (guild_id, user_id) WHERE ended_at IS NULL;

            CREATE INDEX IF NOT EXISTS ix_duty_sessions_guild_started
                ON duty_sessions (guild_id, started_at);
            """;
        command.ExecuteNonQuery();
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static List<DutySession> ReadAll(SqliteCommand command)
    {
        var sessions = new List<DutySession>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            sessions.Add(new DutySession(
                Id: reader.GetInt64(0),
                GuildId: (ulong)reader.GetInt64(1),
                UserId: (ulong)reader.GetInt64(2),
                StartedAt: DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(3)),
                EndedAt: reader.IsDBNull(4) ? null : DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(4)),
                Note: reader.IsDBNull(5) ? null : reader.GetString(5)));
        }
        return sessions;
    }
}
