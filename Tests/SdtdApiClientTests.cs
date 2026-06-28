using System.Net;
using System.Net.Mime;
using System.Text.Json;
using NUnit.Framework;
using PolyhydraGames.SdtdSharp.Models;

namespace PolyhydraGames.SdtdSharp.Tests;

[TestFixture]
public sealed class SdtdApiClientTests
{
    [Test]
    public async Task GetStatsAsync_UsesAuthHeadersAndJsonEndpoint()
    {
        using var handler = new RecordingHandler(_ => JsonSerializer.Serialize(new
        {
            gametime = new { days = 7, hours = 14, minutes = 30 },
            players = 3,
            hostiles = 5,
            animals = 2
        }));
        using var httpClient = new HttpClient(handler);
        var client = new SdtdApiClient(new SdtdServerConfig("example.test", 443, null, "operator", "secret-token"), httpClient);

        var stats = await client.GetStatsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(stats.Players, Is.EqualTo(3));
            Assert.That(handler.Requests, Has.Count.EqualTo(1));
            Assert.That(handler.Requests[0].RequestUri!.ToString(), Is.EqualTo("https://example.test/api/getstats?adminuser=operator&admintoken=secret-token"));
            Assert.That(handler.Requests[0].Headers.GetValues("X-SDTD-API-TOKENNAME").Single(), Is.EqualTo("operator"));
            Assert.That(handler.Requests[0].Headers.GetValues("X-SDTD-API-SECRET").Single(), Is.EqualTo("secret-token"));
        });
    }

    [Test]
    public async Task ExecuteConsoleCommandAsync_AddsEncodedCommandQuery()
    {
        using var handler = new RecordingHandler(_ => JsonSerializer.Serialize(new
        {
            command = "say",
            parameters = "\"hello survivors\"",
            result = "Command executed"
        }));
        using var httpClient = new HttpClient(handler);
        var client = new SdtdApiClient(new SdtdServerConfig("127.0.0.1", 8080, false, "admin", "token"), httpClient);

        var response = await client.ExecuteConsoleCommandAsync("say \"hello survivors\"");

        Assert.Multiple(() =>
        {
            Assert.That(response.Result, Is.EqualTo("Command executed"));
            Assert.That(handler.Requests[0].RequestUri!.AbsolutePath, Is.EqualTo("/api/executeconsolecommand"));
            Assert.That(handler.Requests[0].RequestUri!.Query, Does.Contain("command=say+%22hello+survivors%22"));
        });
    }

    [Test]
    public async Task ReadOnlyEndpoints_UseExpectedPathsAndQueryParameters()
    {
        using var handler = new RecordingHandler(ResponseFor);
        using var httpClient = new HttpClient(handler);
        var client = new SdtdApiClient(new SdtdServerConfig("server.local", 8082, false, "admin", "token"), httpClient);

        await client.GetOnlinePlayersAsync();
        await client.GetAllowedCommandsAsync();
        await client.GetAnimalsLocationAsync();
        await client.GetHostileLocationAsync();
        await client.GetLandClaimsAsync();
        await client.GetLandClaimsAsync("76561198000000000");
        await client.GetPlayerInventoryAsync("76561198000000000");
        await client.GetPlayerInventoryAsync("EOS_abc");
        await client.GetPlayerInventoriesAsync();
        await client.GetPlayerListAsync(rowsPerPage: 25, page: 2);
        await client.GetPlayersLocationAsync(offline: true);
        await client.GetServerInfoAsync();
        await client.GetWebUIUpdatesAsync(latestLine: 12);
        await client.GetLogAsync(firstLine: 8, count: 20);

        var requests = handler.Requests.Select(request => request.RequestUri!).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(requests.Select(uri => uri.AbsolutePath), Is.EqualTo(new[]
            {
                "/api/getlayersonline",
                "/api/getallowedcommands",
                "/api/getanimalslocation",
                "/api/gethostilelocation",
                "/api/getlandclaims",
                "/api/getlandclaims",
                "/api/getplayerinventory",
                "/api/getplayerinventory",
                "/api/getplayerinventories",
                "/api/getplayerlist",
                "/api/getplayerslocation",
                "/api/getserverinfo",
                "/api/getwebuiupdates",
                "/api/getlog"
            }));
            Assert.That(requests[5].Query, Does.Contain("steamid=76561198000000000"));
            Assert.That(requests[6].Query, Does.Contain("steamid=76561198000000000"));
            Assert.That(requests[6].Query, Does.Contain("userid=Steam_76561198000000000"));
            Assert.That(requests[7].Query, Does.Contain("userid=EOS_abc"));
            Assert.That(requests[9].Query, Does.Contain("rowsperpage=25"));
            Assert.That(requests[9].Query, Does.Contain("page=2"));
            Assert.That(requests[10].Query, Does.Contain("offline=True"));
            Assert.That(requests[12].Query, Does.Contain("latestLine=12"));
            Assert.That(requests[13].Query, Does.Contain("firstLine=8"));
            Assert.That(requests[13].Query, Does.Contain("count=20"));
        });
    }

    [Test]
    public void FetchJsonAsync_RejectsNonJsonResponses()
    {
        using var handler = new RecordingHandler(_ => "not json", MediaTypeNames.Text.Plain);
        using var httpClient = new HttpClient(handler);
        var client = new SdtdApiClient(new SdtdServerConfig("server.local", 8082, false, "admin", "token"), httpClient);

        var exception = Assert.ThrowsAsync<Exception>(() => client.GetStatsAsync());

        Assert.That(exception!.Message, Does.Contain("Expected JSON response"));
    }

    [Test]
    public void FetchJsonAsync_RejectsMalformedJsonResponses()
    {
        using var handler = new RecordingHandler(_ => "{not-json");
        using var httpClient = new HttpClient(handler);
        var client = new SdtdApiClient(new SdtdServerConfig("server.local", 8082, false, "admin", "token"), httpClient);

        Assert.ThrowsAsync<JsonException>(() => client.GetStatsAsync());
    }

    private static string ResponseFor(HttpRequestMessage request)
    {
        return request.RequestUri!.AbsolutePath switch
        {
            "/api/getstats" => JsonSerializer.Serialize(new { gametime = GameTimeFixture(), players = 3, hostiles = 5, animals = 2 }),
            "/api/getlayersonline" => JsonSerializer.Serialize(new[] { PlayerFixture() }),
            "/api/getallowedcommands" => JsonSerializer.Serialize(new { commands = new[] { new { command = "say", description = "Sends chat", help = "say <message>" } } }),
            "/api/getanimalslocation" => JsonSerializer.Serialize(new[] { EntityFixture("deer") }),
            "/api/gethostilelocation" => JsonSerializer.Serialize(new[] { EntityFixture("zombie") }),
            "/api/getlandclaims" => JsonSerializer.Serialize(new { claimsize = 41, claimowners = new[] { ClaimOwnerFixture() } }),
            "/api/getplayerinventory" => JsonSerializer.Serialize(new { playername = "Survivor", userid = "Steam_76561198000000000", steamid = "76561198000000000", bag = Array.Empty<object>() }),
            "/api/getplayerinventories" => JsonSerializer.Serialize(new[] { new { playername = "Survivor", userid = "Steam_76561198000000000", steamid = "76561198000000000", bag = Array.Empty<object>() } }),
            "/api/getplayerlist" => JsonSerializer.Serialize(new { total = 1, totalUnfiltered = 1, firstResult = 0, players = new[] { PlayerFixture() } }),
            "/api/getplayerslocation" => JsonSerializer.Serialize(new[] { PlayerLocationFixture() }),
            "/api/getserverinfo" => ServerInfoJson(),
            "/api/getwebuiupdates" => JsonSerializer.Serialize(new { gametime = GameTimeFixture(), players = 1, hostiles = 2, animals = 3, newlogs = 4 }),
            "/api/getlog" => JsonSerializer.Serialize(new { firstLine = 8, lastLine = 28, entries = new[] { new { date = "2026-06-28", time = "01:00:00", uptime = "10", msg = "hello", trace = "", type = "Log" } } }),
            "/api/executeconsolecommand" => JsonSerializer.Serialize(new { command = "say", parameters = "\"hello survivors\"", result = "Command executed" }),
            _ => throw new InvalidOperationException($"Unexpected path: {request.RequestUri.AbsolutePath}")
        };
    }

    private static object GameTimeFixture() => new { days = 7, hours = 14, minutes = 30 };

    private static object PositionFixture() => new { x = 1, y = 2, z = 3 };

    private static object EntityFixture(string name) => new { id = 42, name, position = PositionFixture() };

    private static object PlayerFixture() => new
    {
        steamid = "76561198000000000",
        userid = "Steam_76561198000000000",
        entityid = 1001,
        ip = "127.0.0.1",
        name = "Survivor",
        online = true,
        position = PositionFixture(),
        level = 12,
        health = 90,
        stamina = 80,
        zombiekills = 3,
        playerkills = 0,
        playerdeaths = 1,
        score = 25,
        totalplaytime = 3600,
        lastonline = "2026-06-28T01:00:00Z",
        ping = 12,
        banned = false
    };

    private static object PlayerLocationFixture() => new
    {
        steamid = "76561198000000000",
        userid = "Steam_76561198000000000",
        name = "Survivor",
        online = true,
        position = PositionFixture()
    };

    private static object ClaimOwnerFixture() => new
    {
        steamid = "76561198000000000",
        claimactive = true,
        playername = "Survivor",
        claims = new[] { PositionFixture() }
    };

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

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, string> responseFactory,
        string mediaType = MediaTypeNames.Application.Json) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseFactory(request))
            };
            response.Content.Headers.ContentType = new(mediaType);
            return Task.FromResult(response);
        }
    }
}
