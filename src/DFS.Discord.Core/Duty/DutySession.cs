namespace DFS.Discord.Core.Duty;

/// <summary>
/// One stretch of on-duty time for a member of a guild. Times are stored and compared in UTC.
/// </summary>
public sealed record DutySession(
    long Id,
    ulong GuildId,
    ulong UserId,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    string? Note)
{
    public bool IsOpen => EndedAt is null;

    /// <summary>Length of the session, treating an open session as ending at <paramref name="now"/>.</summary>
    public TimeSpan DurationAt(DateTimeOffset now) => (EndedAt ?? now) - StartedAt;
}
