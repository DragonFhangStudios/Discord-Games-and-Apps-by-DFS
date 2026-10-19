using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace DFS.DutyBot;

/// <summary>Connects to Discord, registers slash commands and routes interactions to modules.</summary>
public sealed class BotWorker(
    DiscordSocketClient client,
    InteractionService interactions,
    IServiceProvider services,
    IOptions<BotOptions> options,
    ILogger<BotWorker> logger) : IHostedService
{
    private readonly BotOptions _options = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Token))
        {
            throw new InvalidOperationException(
                "No bot token configured. Run: dotnet user-secrets set \"Discord:Token\" \"<your token>\" " +
                "--project Bots/DutyBot (or set the Discord__Token environment variable).");
        }

        client.Log += LogAsync;
        interactions.Log += LogAsync;
        client.Ready += RegisterCommandsAsync;
        client.InteractionCreated += HandleInteractionAsync;
        interactions.InteractionExecuted += HandleExecutedAsync;

        await interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), services);

        await client.LoginAsync(TokenType.Bot, _options.Token);
        await client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.StopAsync();
        await client.LogoutAsync();
    }

    private async Task RegisterCommandsAsync()
    {
        if (_options.DevGuildId is { } guildId)
        {
            await interactions.RegisterCommandsToGuildAsync(guildId);
            logger.LogInformation("Registered slash commands to test server {GuildId}", guildId);
        }
        else
        {
            await interactions.RegisterCommandsGloballyAsync();
            logger.LogInformation("Registered slash commands globally");
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(client, interaction);
        await interactions.ExecuteCommandAsync(context, services);
    }

    private static async Task HandleExecutedAsync(ICommandInfo command, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess)
            return;

        var message = result.Error switch
        {
            InteractionCommandError.UnmetPrecondition => $"⛔ {result.ErrorReason}",
            _ => "⚠️ Something went wrong running that command. Please try again.",
        };

        if (context.Interaction.HasResponded)
            await context.Interaction.FollowupAsync(message, ephemeral: true);
        else
            await context.Interaction.RespondAsync(message, ephemeral: true);
    }

    private Task LogAsync(LogMessage message)
    {
        var level = message.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            _ => LogLevel.Trace,
        };
        logger.Log(level, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
        return Task.CompletedTask;
    }
}
