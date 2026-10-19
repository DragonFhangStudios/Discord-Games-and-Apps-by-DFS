# DFS Duty Bot

Staff duty tracking for Discord communities (RP servers, FiveM departments, support teams, mod teams).
Members clock on and off with slash commands; leads get a per-member time report.

## Commands

| Command | Who | What it does |
|---|---|---|
| `/duty on [note]` | Anyone | Clock on, with an optional note like "patrol" or "tickets". |
| `/duty off` | Anyone | Clock off and show how long the shift lasted. |
| `/duty roster` | Anyone | List who is on duty right now and for how long. |
| `/duty report [days]` | Manage Server | Total duty time per member over the last 1–90 days (default 7). Shown only to the person who ran it. |

All times are stored in UTC and shown with Discord timestamps, so every viewer sees their own local time.
Public messages never ping anyone.

## Run it locally

1. Create an application at <https://discord.com/developers/applications>, add a **Bot**, and copy its token.
2. Invite it to your test server with the `bot` and `applications.commands` scopes (no extra permissions needed).
3. Store the token outside the repo:

   ```bash
   dotnet user-secrets set "Discord:Token" "<your token>" --project Bots/DutyBot
   ```

4. Optional but recommended while testing: register commands to your test server so they appear instantly.
   Turn on Developer Mode in Discord, right-click the server → **Copy Server ID**, then:

   ```bash
   dotnet user-secrets set "Discord:DevGuildId" "<server id>" --project Bots/DutyBot
   ```

5. Run it:

   ```bash
   dotnet run --project Bots/DutyBot
   ```

Duty data goes in `dutybot.db` next to the app (change it with `Storage:DatabasePath`).
On a server or Raspberry Pi, use environment variables instead of user-secrets: `Discord__Token`, `Discord__DevGuildId`, `Storage__DatabasePath`.

## How it's built

- **`src/DFS.Discord.Core`**: duty rules (`DutyService`) and SQLite storage. No Discord dependency, so it's fully unit-tested.
- **`Bots/DutyBot`**: a thin Discord.Net host that turns slash commands into `DutyService` calls.
- **`Tests/DFS.Discord.Core.Tests`**: xUnit tests using a fake clock to cover shift lengths, report windows and edge cases.
