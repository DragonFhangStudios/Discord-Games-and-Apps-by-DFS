namespace DFS.Discord.Core.Duty;

public enum ClockOnStatus { Started, AlreadyOnDuty }

public sealed record ClockOnResult(ClockOnStatus Status, DutySession Session);

public enum ClockOffStatus { Ended, NotOnDuty }

public sealed record ClockOffResult(ClockOffStatus Status, DutySession? Session, TimeSpan Duration);

public sealed record RosterEntry(ulong UserId, DateTimeOffset StartedAt, TimeSpan Elapsed, string? Note);

public sealed record ReportEntry(ulong UserId, TimeSpan Total, int SessionCount);

public sealed record DutyReport(DateTimeOffset From, DateTimeOffset To, IReadOnlyList<ReportEntry> Entries);

/// <summary>
/// Duty tracking rules, independent of Discord. All "now" values come from the injected
/// <see cref="TimeProvider"/> so behaviour can be tested with a fake clock.
/// </summary>
public sealed class DutyService(IDutyStore store, TimeProvider time)
{
    public const int MaxNoteLength = 100;

    public ClockOnResult ClockOn(ulong guildId, ulong userId, string? note = null)
    {
        var started = store.TryStartSession(guildId, userId, time.GetUtcNow(), CleanNote(note));
        if (started is not null)
            return new ClockOnResult(ClockOnStatus.Started, started);

        var existing = store.GetOpenSession(guildId, userId)
            ?? throw new InvalidOperationException("Duty session could not be started or found.");
        return new ClockOnResult(ClockOnStatus.AlreadyOnDuty, existing);
    }

    public ClockOffResult ClockOff(ulong guildId, ulong userId)
    {
        var open = store.GetOpenSession(guildId, userId);
        var ended = open is null ? null : store.TryEndSession(open.Id, time.GetUtcNow());
        if (ended is null)
            return new ClockOffResult(ClockOffStatus.NotOnDuty, null, TimeSpan.Zero);

        return new ClockOffResult(ClockOffStatus.Ended, ended, ended.DurationAt(ended.EndedAt!.Value));
    }

    public IReadOnlyList<RosterEntry> GetRoster(ulong guildId)
    {
        var now = time.GetUtcNow();
        return store.GetOpenSessions(guildId)
            .Select(s => new RosterEntry(s.UserId, s.StartedAt, s.DurationAt(now), s.Note))
            .ToList();
    }

    /// <summary>
    /// Totals each member's duty time inside the last <paramref name="window"/>. Sessions that
    /// cross the window edge only count the part inside it; open sessions count up to now.
    /// </summary>
    public DutyReport GetReport(ulong guildId, TimeSpan window)
    {
        if (window <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(window), "Report window must be positive.");

        var to = time.GetUtcNow();
        var from = to - window;

        var entries = store.GetSessionsOverlapping(guildId, from, to)
            .GroupBy(s => s.UserId)
            .Select(g => new ReportEntry(
                g.Key,
                g.Aggregate(TimeSpan.Zero, (total, s) => total + Overlap(s, from, to)),
                g.Count()))
            .OrderByDescending(e => e.Total)
            .ThenBy(e => e.UserId)
            .ToList();

        return new DutyReport(from, to, entries);
    }

    private static TimeSpan Overlap(DutySession session, DateTimeOffset from, DateTimeOffset to)
    {
        var start = session.StartedAt > from ? session.StartedAt : from;
        var end = session.EndedAt is { } e && e < to ? e : to;
        return end > start ? end - start : TimeSpan.Zero;
    }

    private static string? CleanNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return null;

        var trimmed = note.Trim();
        return trimmed.Length <= MaxNoteLength ? trimmed : trimmed[..MaxNoteLength];
    }
}
