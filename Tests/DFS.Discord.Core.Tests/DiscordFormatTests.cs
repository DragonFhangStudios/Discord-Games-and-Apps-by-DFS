namespace DFS.Discord.Core.Tests;

public sealed class DiscordFormatTests
{
    [Theory]
    [InlineData(0, "0m")]
    [InlineData(45, "45m")]
    [InlineData(60, "1h 0m")]
    [InlineData(125, "2h 5m")]
    [InlineData(26 * 60 + 3, "26h 3m")]
    [InlineData(-5, "0m")]
    public void Duration_IsCompact(int minutes, string expected)
    {
        Assert.Equal(expected, DiscordFormat.Duration(TimeSpan.FromMinutes(minutes)));
    }

    [Fact]
    public void Timestamp_UsesUnixSecondsAndStyle()
    {
        var value = new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

        Assert.Equal("<t:1790856000:R>", DiscordFormat.Timestamp(value));
        Assert.Equal("<t:1790856000:f>", DiscordFormat.Timestamp(value, 'f'));
    }

    [Fact]
    public void Mention_WrapsUserId()
    {
        Assert.Equal("<@42>", DiscordFormat.Mention(42));
    }
}
