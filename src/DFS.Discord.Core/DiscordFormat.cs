namespace DFS.Discord.Core;

/// <summary>Small helpers for Discord's markdown conventions.</summary>
public static class DiscordFormat
{
    /// <summary>
    /// A Discord timestamp tag. Discord renders it in each viewer's own time zone,
    /// so the bot never has to know where its users are.
    /// Styles: R = relative ("2 hours ago"), f = short date/time, t = short time.
    /// </summary>
    public static string Timestamp(DateTimeOffset value, char style = 'R') =>
        $"<t:{value.ToUnixTimeSeconds()}:{style}>";

    public static string Mention(ulong userId) => $"<@{userId}>";

    /// <summary>Compact duration such as "45m", "2h 5m" or "26h 0m".</summary>
    public static string Duration(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
            value = TimeSpan.Zero;

        var hours = (long)value.TotalHours;
        return hours > 0 ? $"{hours}h {value.Minutes}m" : $"{value.Minutes}m";
    }
}
