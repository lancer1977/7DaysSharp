using System.Net;
using System.Text.Json;
using NUnit.Framework;
using PolyhydraGames.SdtdSharp.Models;

namespace PolyhydraGames.SdtdSharp.Tests;

[TestFixture]
public sealed class SdtdDiagnosticsProbeTests
{
    [Test]
    public async Task CheckReadinessAsync_ReturnsReadyWhenReadOnlyEndpointsRespond()
    {
        using var handler = new DiagnosticsHandler(request => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent(request.RequestUri!.AbsolutePath)
        });
        var probe = CreateProbe(handler);

        var report = await probe.CheckReadinessAsync();

        Assert.Multiple(() =>
        {
            Assert.That(report.IsReady, Is.True);
            Assert.That(report.Checks.Select(check => check.Name), Is.EqualTo(new[] { "server-info", "stats", "allowed-commands" }));
            Assert.That(report.Checks.Select(check => check.Status), Is.All.EqualTo(SdtdDiagnosticsStatus.Ready));
        });
    }

    [Test]
    public async Task CheckReadinessAsync_ClassifiesAuthenticationFailure()
    {
        using var handler = new DiagnosticsHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("unauthorized")
        });
        var probe = CreateProbe(handler);

        var report = await probe.CheckReadinessAsync();

        Assert.Multiple(() =>
        {
            Assert.That(report.IsReady, Is.False);
            Assert.That(report.Checks.Select(check => check.Status), Is.All.EqualTo(SdtdDiagnosticsStatus.AuthFailed));
        });
    }

    [Test]
    public async Task CheckReadinessAsync_ClassifiesMalformedJson()
    {
        using var handler = new DiagnosticsHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{not-json")
        });
        var probe = CreateProbe(handler);

        var report = await probe.CheckReadinessAsync();

        Assert.Multiple(() =>
        {
            Assert.That(report.IsReady, Is.False);
            Assert.That(report.Checks.Select(check => check.Status), Is.All.EqualTo(SdtdDiagnosticsStatus.ParseFailed));
        });
    }

    [Test]
    public async Task CheckReadinessAsync_ClassifiesConnectionFailure()
    {
        using var handler = new DiagnosticsHandler(_ => throw new HttpRequestException("connection refused"));
        var probe = CreateProbe(handler);

        var report = await probe.CheckReadinessAsync();

        Assert.Multiple(() =>
        {
            Assert.That(report.IsReady, Is.False);
            Assert.That(report.Checks.Select(check => check.Status), Is.All.EqualTo(SdtdDiagnosticsStatus.ConnectionFailed));
        });
    }

    private static SdtdDiagnosticsProbe CreateProbe(HttpMessageHandler handler)
    {
        var client = new SdtdApiClient(new SdtdServerConfig("server.local", 8082, false, "admin", "token"), new HttpClient(handler));
        return new SdtdDiagnosticsProbe(client);
    }

    private static StringContent JsonContent(string path)
    {
        var content = path switch
        {
            "/api/getserverinfo" => ServerInfoJson(),
            "/api/getstats" => JsonSerializer.Serialize(new { gametime = new { days = 7, hours = 14, minutes = 30 }, players = 3, hostiles = 5, animals = 2 }),
            "/api/getallowedcommands" => JsonSerializer.Serialize(new { commands = new[] { new { command = "say", description = "Sends chat", help = "say <message>" } } }),
            _ => throw new InvalidOperationException($"Unexpected path: {path}")
        };
        var stringContent = new StringContent(content);
        stringContent.Headers.ContentType = new("application/json");
        return stringContent;
    }

    private static string ServerInfoJson()
    {
        var fields = new[]
        {
            "GameType", "GameName", "GameHost", "ServerDescription", "ServerWebsiteURL", "LevelName", "GameMode",
            "Version", "IP", "CountryCode", "SteamID", "CompatibilityVersion", "Platform", "Port", "CurrentPlayers",
            "MaxPlayers", "GameDifficulty", "DayNightLength", "ZombiesRun", "DayCount", "Ping", "DropOnDeath",
            "DropOnQuit", "BloodMoonEnemyCount", "EnemyDifficulty", "PlayerKillingMode", "CurrentServerTime",
            "DayLightLength", "BlockDurabilityModifier", "AirDropFrequency", "LootAbundance", "LootRespawnDays",
            "MaxSpawnedZombies", "LandClaimSize", "LandClaimDeadZone", "LandClaimExpiryTime", "LandClaimDecayMode",
            "LandClaimOnlineDurabilityModifier", "LandClaimOfflineDurabilityModifier", "MaxSpawnedAnimals",
            "IsDedicated", "IsPasswordProtected", "ShowFriendPlayerOnMap", "BuildCreate", "EACEnabled",
            "Architecture64", "StockSettings", "StockFiles", "RequiresMod", "AirDropMarker", "EnemySpawnMode",
            "IsPublic"
        };
        var entries = fields.Select(field => $"\"{field}\":{{\"type\":\"string\",\"value\":\"{field}-value\"}}");
        return "{" + string.Join(",", entries) + "}";
    }

    private sealed class DiagnosticsHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responseFactory(request));
    }
}
