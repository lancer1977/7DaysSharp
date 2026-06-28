using System.Net;
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

    private sealed class RecordingHandler(Func<HttpRequestMessage, string> responseFactory) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseFactory(request))
            };
            response.Content.Headers.ContentType = new("application/json");
            return Task.FromResult(response);
        }
    }
}
