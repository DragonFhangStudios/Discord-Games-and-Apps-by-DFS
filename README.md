# Discord Games and Apps by DFS
 DFS Coding Resource for Discord

## Projects

| Project | What it is |
|---|---|
| [`Bots/DutyBot`](Bots/DutyBot/README.md) | Staff duty tracker bot: `/duty on`, `/duty off`, `/duty roster`, `/duty report`. |
| `src/DFS.Discord.Core` | Shared, Discord-independent bot logic and storage. |
| `src/DFS.Simulation.Core` | Grid simulation engine. |
| `Tests/` | xUnit test projects. |

## Build and test

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet test "Discord Games and Apps by DFS.sln"
```

On macOS/Linux, add `-p:EnableWindowsTargeting=true` so the WPF app at the repo root builds too.
