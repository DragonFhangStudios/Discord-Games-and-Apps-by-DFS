using Microsoft.Data.Sqlite;

namespace DFS.Discord.Core.Tests;

/// <summary>A throwaway SQLite file per test, deleted afterwards.</summary>
public sealed class TempDatabase : IDisposable
{
    public string Path { get; } =
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"dfs-duty-{Guid.NewGuid():N}.db");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        foreach (var suffix in new[] { "", "-wal", "-shm" })
            File.Delete(Path + suffix);
    }
}
