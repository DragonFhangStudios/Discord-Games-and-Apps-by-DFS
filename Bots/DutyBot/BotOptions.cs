namespace DFS.DutyBot;

public sealed class BotOptions
{
    /// <summary>Bot token. Never commit it: set it with user-secrets or the Discord__Token environment variable.</summary>
    public string Token { get; set; } = "";

    /// <summary>
    /// Optional test server ID. When set, commands register to that server only and update instantly;
    /// when empty, they register globally (can take a while to appear).
    /// </summary>
    public ulong? DevGuildId { get; set; }
}
