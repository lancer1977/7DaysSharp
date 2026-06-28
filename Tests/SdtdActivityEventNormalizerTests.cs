using NUnit.Framework;
using PolyhydraGames.SdtdSharp.Models;

namespace PolyhydraGames.SdtdSharp.Tests;

[TestFixture]
public sealed class SdtdActivityEventNormalizerTests
{
    [Test]
    public void FromLog_MapsKnownAndUnknownLogRows()
    {
        var log = new GetLog(
            40,
            44,
            new List<LogLine>
            {
                new("2026-06-28", "02:00:00", "1", "Player 'Jordan' joined the game", "", "Log"),
                new("2026-06-28", "02:00:01", "2", "Jordan: hello survivors", "", "Chat"),
                new("2026-06-28", "02:00:02", "3", "Executing command: listplayers", "", "Command"),
                new("2026-06-28", "02:00:03", "4", "Player 'Jordan' left the game", "", "Log"),
                new("2026-06-28", "02:00:04", "5", "A new unclassified line", "", "Log")
            });

        var events = SdtdActivityEventNormalizer.FromLog(log);

        Assert.Multiple(() =>
        {
            Assert.That(events.Select(e => e.Kind), Is.EqualTo(new[]
            {
                SdtdActivityEventKinds.PlayerJoined,
                SdtdActivityEventKinds.Chat,
                SdtdActivityEventKinds.CommandOutput,
                SdtdActivityEventKinds.PlayerLeft,
                SdtdActivityEventKinds.Unknown
            }));
            Assert.That(events[0].PlayerName, Is.EqualTo("Jordan"));
            Assert.That(events[1].PlayerName, Is.EqualTo("Jordan"));
            Assert.That(events[2].Command, Is.EqualTo("listplayers"));
            Assert.That(events[4].Message, Is.EqualTo("A new unclassified line"));
        });
    }

    [Test]
    public void FromWebUiUpdates_MapsSnapshotToStableActivityEvent()
    {
        var response = new GetWebUIUpdatesResponse(new GameTime(7, 14, 30), Players: 3, Hostiles: 5, Animals: 2, Newlogs: 4);

        var activityEvent = SdtdActivityEventNormalizer.FromWebUiUpdates(response);

        Assert.Multiple(() =>
        {
            Assert.That(activityEvent.Kind, Is.EqualTo(SdtdActivityEventKinds.WebUiUpdate));
            Assert.That(activityEvent.EventId, Is.EqualTo("webui:7:14:30:4"));
            Assert.That(activityEvent.NewLogCount, Is.EqualTo(4));
            Assert.That(activityEvent.Message, Does.Contain("players=3"));
        });
    }
}
