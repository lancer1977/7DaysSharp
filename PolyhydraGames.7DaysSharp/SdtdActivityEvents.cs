using System.Text.RegularExpressions;
using PolyhydraGames.SdtdSharp.Models;

namespace PolyhydraGames.SdtdSharp;

public sealed record SdtdActivityEvent(
    string EventId,
    string Kind,
    string Source,
    string Message,
    string? PlayerName = null,
    string? Command = null,
    int? NewLogCount = null);

public static class SdtdActivityEventKinds
{
    public const string PlayerJoined = "player-joined";
    public const string PlayerLeft = "player-left";
    public const string Chat = "chat";
    public const string CommandOutput = "command-output";
    public const string WebUiUpdate = "web-ui-update";
    public const string Unknown = "unknown";
}

public static partial class SdtdActivityEventNormalizer
{
    public static IReadOnlyList<SdtdActivityEvent> FromLog(GetLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        return log.entries
            .Select((entry, index) => FromLogLine(entry, log.FirstLine + index))
            .ToArray();
    }

    public static SdtdActivityEvent FromWebUiUpdates(GetWebUIUpdatesResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new SdtdActivityEvent(
            EventId: $"webui:{response.Gametime.Days}:{response.Gametime.Hours}:{response.Gametime.Minutes}:{response.Newlogs}",
            Kind: SdtdActivityEventKinds.WebUiUpdate,
            Source: "web-ui-updates",
            Message: $"players={response.Players};hostiles={response.Hostiles};animals={response.Animals};newlogs={response.Newlogs}",
            NewLogCount: response.Newlogs);
    }

    private static SdtdActivityEvent FromLogLine(LogLine line, int lineNumber)
    {
        var eventId = $"log:{lineNumber}:{line.Date}:{line.Time}";
        var message = line.Msg ?? string.Empty;

        var playerJoined = PlayerJoinedPattern().Match(message);
        if (playerJoined.Success)
        {
            return new SdtdActivityEvent(eventId, SdtdActivityEventKinds.PlayerJoined, "log", message, PlayerName: playerJoined.Groups["name"].Value);
        }

        var playerLeft = PlayerLeftPattern().Match(message);
        if (playerLeft.Success)
        {
            return new SdtdActivityEvent(eventId, SdtdActivityEventKinds.PlayerLeft, "log", message, PlayerName: playerLeft.Groups["name"].Value);
        }

        var command = CommandPattern().Match(message);
        if (command.Success || line.Type.Equals("command", StringComparison.OrdinalIgnoreCase))
        {
            return new SdtdActivityEvent(eventId, SdtdActivityEventKinds.CommandOutput, "log", message, Command: command.Success ? command.Groups["command"].Value : null);
        }

        var chat = ChatPattern().Match(message);
        if (chat.Success)
        {
            return new SdtdActivityEvent(eventId, SdtdActivityEventKinds.Chat, "log", message, PlayerName: chat.Groups["name"].Value);
        }

        return new SdtdActivityEvent(eventId, SdtdActivityEventKinds.Unknown, "log", message);
    }

    [GeneratedRegex(@"Player\s+'?(?<name>[^']+)'?\s+(joined|connected)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PlayerJoinedPattern();

    [GeneratedRegex(@"Player\s+'?(?<name>[^']+)'?\s+(left|disconnected)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PlayerLeftPattern();

    [GeneratedRegex(@"^\s*(?<name>[^:]+):\s+.+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ChatPattern();

    [GeneratedRegex(@"^(Executing command|Command):\s*(?<command>.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CommandPattern();
}
