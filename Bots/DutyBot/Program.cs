using DFS.Discord.Core.Duty;
using DFS.DutyBot;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true);

builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("Discord"));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IDutyStore>(_ =>
    new SqliteDutyStore(builder.Configuration["Storage:DatabasePath"] ?? "dutybot.db"));
builder.Services.AddSingleton<DutyService>();

builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    // Slash commands only need the Guilds intent; no privileged intents required.
    GatewayIntents = GatewayIntents.Guilds,
}));
builder.Services.AddSingleton(sp => new InteractionService(sp.GetRequiredService<DiscordSocketClient>()));
builder.Services.AddHostedService<BotWorker>();

await builder.Build().RunAsync();
