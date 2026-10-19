using DFS.Discord.Core.Duty;
using Microsoft.Extensions.Time.Testing;

namespace DFS.Discord.Core.Tests;

public sealed class DutyServiceTests : IDisposable
{
    private const ulong Guild = 1000;
    private const ulong OtherGuild = 2000;
    private const ulong Alice = 11;
    private const ulong Bob = 22;

    private static readonly DateTimeOffset Start = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly TempDatabase _db = new();
    private readonly FakeTimeProvider _clock = new(Start);
    private readonly DutyService _duty;

    public DutyServiceTests()
    {
        _duty = new DutyService(new SqliteDutyStore(_db.Path), _clock);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public void ClockOn_StartsSessionAtCurrentTime()
    {
        var result = _duty.ClockOn(Guild, Alice, "patrol");

        Assert.Equal(ClockOnStatus.Started, result.Status);
        Assert.Equal(Start, result.Session.StartedAt);
        Assert.Equal("patrol", result.Session.Note);
        Assert.True(result.Session.IsOpen);
    }

    [Fact]
    public void ClockOn_Twice_ReturnsExistingSession()
    {
        var first = _duty.ClockOn(Guild, Alice);
        _clock.Advance(TimeSpan.FromMinutes(5));

        var second = _duty.ClockOn(Guild, Alice);

        Assert.Equal(ClockOnStatus.AlreadyOnDuty, second.Status);
        Assert.Equal(first.Session.Id, second.Session.Id);
        Assert.Equal(Start, second.Session.StartedAt);
    }

    [Fact]
    public void ClockOff_WithoutClockOn_ReportsNotOnDuty()
    {
        var result = _duty.ClockOff(Guild, Alice);

        Assert.Equal(ClockOffStatus.NotOnDuty, result.Status);
        Assert.Null(result.Session);
    }

    [Fact]
    public void ClockOff_ReturnsSessionDuration()
    {
        _duty.ClockOn(Guild, Alice);
        _clock.Advance(TimeSpan.FromMinutes(90));

        var result = _duty.ClockOff(Guild, Alice);

        Assert.Equal(ClockOffStatus.Ended, result.Status);
        Assert.Equal(TimeSpan.FromMinutes(90), result.Duration);
        Assert.Equal(Start.AddMinutes(90), result.Session!.EndedAt);
    }

    [Fact]
    public void ClockOff_ThenClockOnAgain_StartsNewSession()
    {
        var first = _duty.ClockOn(Guild, Alice);
        _duty.ClockOff(Guild, Alice);

        var second = _duty.ClockOn(Guild, Alice);

        Assert.Equal(ClockOnStatus.Started, second.Status);
        Assert.NotEqual(first.Session.Id, second.Session.Id);
    }

    [Fact]
    public void Sessions_AreIndependentPerGuild()
    {
        _duty.ClockOn(Guild, Alice);

        var other = _duty.ClockOn(OtherGuild, Alice);

        Assert.Equal(ClockOnStatus.Started, other.Status);
        Assert.Single(_duty.GetRoster(Guild));
        Assert.Single(_duty.GetRoster(OtherGuild));
    }

    [Fact]
    public void ClockOn_TrimsBlankAndLongNotes()
    {
        var blank = _duty.ClockOn(Guild, Alice, "   ");
        var longNote = _duty.ClockOn(Guild, Bob, "  " + new string('x', 150) + "  ");

        Assert.Null(blank.Session.Note);
        Assert.Equal(DutyService.MaxNoteLength, longNote.Session.Note!.Length);
    }

    [Fact]
    public void Roster_ListsOnlyOpenSessions_OldestFirst_WithElapsedTime()
    {
        _duty.ClockOn(Guild, Alice);
        _clock.Advance(TimeSpan.FromMinutes(10));
        _duty.ClockOn(Guild, Bob);
        _clock.Advance(TimeSpan.FromMinutes(20));

        var roster = _duty.GetRoster(Guild);

        Assert.Collection(roster,
            a => { Assert.Equal(Alice, a.UserId); Assert.Equal(TimeSpan.FromMinutes(30), a.Elapsed); },
            b => { Assert.Equal(Bob, b.UserId); Assert.Equal(TimeSpan.FromMinutes(20), b.Elapsed); });

        _duty.ClockOff(Guild, Alice);
        Assert.Equal(Bob, Assert.Single(_duty.GetRoster(Guild)).UserId);
    }

    [Fact]
    public void Report_TotalsSessionsPerMember_SortedByTimeDescending()
    {
        _duty.ClockOn(Guild, Alice);
        _clock.Advance(TimeSpan.FromHours(1));
        _duty.ClockOff(Guild, Alice);

        _duty.ClockOn(Guild, Bob);
        _clock.Advance(TimeSpan.FromHours(2));
        _duty.ClockOff(Guild, Bob);

        _duty.ClockOn(Guild, Alice);
        _clock.Advance(TimeSpan.FromMinutes(30));
        _duty.ClockOff(Guild, Alice);

        var report = _duty.GetReport(Guild, TimeSpan.FromDays(7));

        Assert.Collection(report.Entries,
            b => { Assert.Equal(Bob, b.UserId); Assert.Equal(TimeSpan.FromHours(2), b.Total); Assert.Equal(1, b.SessionCount); },
            a => { Assert.Equal(Alice, a.UserId); Assert.Equal(TimeSpan.FromMinutes(90), a.Total); Assert.Equal(2, a.SessionCount); });
    }

    [Fact]
    public void Report_OnlyCountsTimeInsideWindow()
    {
        // Session runs 3h; the report window only covers its last hour.
        _duty.ClockOn(Guild, Alice);
        _clock.Advance(TimeSpan.FromHours(3));
        _duty.ClockOff(Guild, Alice);

        var report = _duty.GetReport(Guild, TimeSpan.FromHours(1));

        Assert.Equal(TimeSpan.FromHours(1), Assert.Single(report.Entries).Total);
    }

    [Fact]
    public void Report_ExcludesSessionsEndedBeforeWindow()
    {
        _duty.ClockOn(Guild, Alice);
        _clock.Advance(TimeSpan.FromHours(1));
        _duty.ClockOff(Guild, Alice);
        _clock.Advance(TimeSpan.FromDays(10));

        var report = _duty.GetReport(Guild, TimeSpan.FromDays(7));

        Assert.Empty(report.Entries);
    }

    [Fact]
    public void Report_CountsOpenSessionUpToNow()
    {
        _duty.ClockOn(Guild, Alice);
        _clock.Advance(TimeSpan.FromMinutes(45));

        var report = _duty.GetReport(Guild, TimeSpan.FromDays(1));

        Assert.Equal(TimeSpan.FromMinutes(45), Assert.Single(report.Entries).Total);
        Assert.Equal(_clock.GetUtcNow(), report.To);
    }

    [Fact]
    public void Report_RejectsNonPositiveWindow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _duty.GetReport(Guild, TimeSpan.Zero));
    }
}
