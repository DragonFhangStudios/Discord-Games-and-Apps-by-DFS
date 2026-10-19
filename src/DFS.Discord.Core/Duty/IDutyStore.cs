namespace DFS.Discord.Core.Duty;

public interface IDutyStore
{
    DutySession? GetOpenSession(ulong guildId, ulong userId);

    /// <summary>
    /// Opens a new session. Returns null if the member already has an open session in that guild.
    /// </summary>
    DutySession? TryStartSession(ulong guildId, ulong userId, DateTimeOffset startedAt, string? note);

    /// <summary>Closes an open session. Returns null if it doesn't exist or is already closed.</summary>
    DutySession? TryEndSession(long sessionId, DateTimeOffset endedAt);

    IReadOnlyList<DutySession> GetOpenSessions(ulong guildId);

    /// <summary>Sessions that overlap [from, to), including ones still open.</summary>
    IReadOnlyList<DutySession> GetSessionsOverlapping(ulong guildId, DateTimeOffset from, DateTimeOffset to);
}
