using System.Text;
using DFS.Discord.Core;
using DFS.Discord.Core.Duty;
using Discord;
using Discord.Interactions;

namespace DFS.DutyBot;

[Group("duty", "Track staff on-duty time.")]
[CommandContextType(InteractionContextType.Guild)]
public sealed class DutyModule(DutyService duty) : InteractionModuleBase<SocketInteractionContext>
{
    private const int MaxReportRows = 25;

    private static readonly Color OnColor = new(0x2ECC71);
    private static readonly Color InfoColor = new(0x3498DB);

    [SlashCommand("on", "Clock on duty.")]
    public async Task OnAsync(
        [Summary("note", "What you're covering, e.g. \"patrol\" or \"tickets\".")]
        [MaxLength(DutyService.MaxNoteLength)]
        string? note = null)
    {
        var result = duty.ClockOn(Context.Guild.Id, Context.User.Id, note);

        if (result.Status == ClockOnStatus.AlreadyOnDuty)
        {
            await RespondAsync(
                $"You're already on duty (since {DiscordFormat.Timestamp(result.Session.StartedAt)}). Use `/duty off` first.",
                ephemeral: true);
            return;
        }

        var text = $"🟢 {DiscordFormat.Mention(Context.User.Id)} is now **on duty**";
        if (result.Session.Note is { } n)
            text += $" — {n}";
        await RespondAsync(text, allowedMentions: AllowedMentions.None);
    }

    [SlashCommand("off", "Clock off duty.")]
    public async Task OffAsync()
    {
        var result = duty.ClockOff(Context.Guild.Id, Context.User.Id);

        if (result.Status == ClockOffStatus.NotOnDuty)
        {
            await RespondAsync("You're not on duty right now. Use `/duty on` to start.", ephemeral: true);
            return;
        }

        await RespondAsync(
            $"🔴 {DiscordFormat.Mention(Context.User.Id)} is now **off duty** after **{DiscordFormat.Duration(result.Duration)}**.",
            allowedMentions: AllowedMentions.None);
    }

    [SlashCommand("roster", "Show who is on duty right now.")]
    public async Task RosterAsync()
    {
        var roster = duty.GetRoster(Context.Guild.Id);

        if (roster.Count == 0)
        {
            await RespondAsync("Nobody is on duty right now.", ephemeral: true);
            return;
        }

        var lines = new StringBuilder();
        foreach (var entry in roster.Take(MaxReportRows))
        {
            lines.Append($"• {DiscordFormat.Mention(entry.UserId)} — on since {DiscordFormat.Timestamp(entry.StartedAt, 't')} ({DiscordFormat.Duration(entry.Elapsed)})");
            if (entry.Note is { } n)
                lines.Append($" · {n}");
            lines.AppendLine();
        }
        if (roster.Count > MaxReportRows)
            lines.AppendLine($"…and {roster.Count - MaxReportRows} more.");

        var embed = new EmbedBuilder()
            .WithTitle($"On duty now ({roster.Count})")
            .WithDescription(lines.ToString())
            .WithColor(OnColor)
            .Build();
        await RespondAsync(embed: embed, allowedMentions: AllowedMentions.None);
    }

    [SlashCommand("report", "Total duty time per member.")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public async Task ReportAsync(
        [Summary("days", "How many days back to include (1-90).")]
        [MinValue(1), MaxValue(90)]
        int days = 7)
    {
        var report = duty.GetReport(Context.Guild.Id, TimeSpan.FromDays(days));

        var lines = new StringBuilder();
        lines.AppendLine($"Since {DiscordFormat.Timestamp(report.From, 'f')}");
        lines.AppendLine();

        if (report.Entries.Count == 0)
            lines.AppendLine("No duty time recorded in this period.");

        var rank = 1;
        foreach (var entry in report.Entries.Take(MaxReportRows))
        {
            var sessions = entry.SessionCount == 1 ? "1 session" : $"{entry.SessionCount} sessions";
            lines.AppendLine($"**{rank++}.** {DiscordFormat.Mention(entry.UserId)} — {DiscordFormat.Duration(entry.Total)} ({sessions})");
        }
        if (report.Entries.Count > MaxReportRows)
            lines.AppendLine($"…and {report.Entries.Count - MaxReportRows} more.");

        var embed = new EmbedBuilder()
            .WithTitle($"Duty report — last {days} day{(days == 1 ? "" : "s")}")
            .WithDescription(lines.ToString())
            .WithColor(InfoColor)
            .WithFooter("Times show in your own time zone.")
            .Build();
        await RespondAsync(embed: embed, ephemeral: true);
    }
}
