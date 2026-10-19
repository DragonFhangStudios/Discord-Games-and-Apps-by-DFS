using DFS.Discord.Core.Duty;

namespace DFS.Discord.Core.Tests;

public sealed class SqliteDutyStoreTests : IDisposable
{
    private static readonly DateTimeOffset Start = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly TempDatabase _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public void OnlyOneOpenSessionPerMember_EvenWhenCalledDirectly()
    {
        var store = new SqliteDutyStore(_db.Path);

        var first = store.TryStartSession(1, 2, Start, null);
        var second = store.TryStartSession(1, 2, Start.AddMinutes(1), null);

        Assert.NotNull(first);
        Assert.Null(second);
    }

    [Fact]
    public void EndingAClosedSession_ReturnsNull()
    {
        var store = new SqliteDutyStore(_db.Path);
        var session = store.TryStartSession(1, 2, Start, null)!;

        Assert.NotNull(store.TryEndSession(session.Id, Start.AddHours(1)));
        Assert.Null(store.TryEndSession(session.Id, Start.AddHours(2)));
    }

    [Fact]
    public void Data_SurvivesReopeningTheDatabase()
    {
        new SqliteDutyStore(_db.Path).TryStartSession(1, 2, Start, "night shift");

        var reopened = new SqliteDutyStore(_db.Path).GetOpenSession(1, 2);

        Assert.NotNull(reopened);
        Assert.Equal(Start, reopened.StartedAt);
        Assert.Equal("night shift", reopened.Note);
    }

    [Fact]
    public void LargeDiscordIds_RoundTrip()
    {
        // Real snowflakes are 18-19 digits; make sure nothing is truncated on the way through SQLite.
        const ulong guild = 1_234_567_890_123_456_789;
        const ulong user = 987_654_321_098_765_432;
        var store = new SqliteDutyStore(_db.Path);

        store.TryStartSession(guild, user, Start, null);
        var session = store.GetOpenSession(guild, user);

        Assert.NotNull(session);
        Assert.Equal(guild, session.GuildId);
        Assert.Equal(user, session.UserId);
    }
}
