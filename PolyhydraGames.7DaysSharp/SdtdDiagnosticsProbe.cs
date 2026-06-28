using System.Net;
using System.Text.Json;

namespace PolyhydraGames.SdtdSharp;

public sealed record SdtdDiagnosticsReport(IReadOnlyList<SdtdDiagnosticsCheck> Checks)
{
    public bool IsReady => Checks.All(check => check.Status == SdtdDiagnosticsStatus.Ready);
}

public sealed record SdtdDiagnosticsCheck(string Name, string Status, string Message);

public static class SdtdDiagnosticsStatus
{
    public const string Ready = "ready";
    public const string AuthFailed = "auth-failed";
    public const string HttpFailed = "http-failed";
    public const string ParseFailed = "parse-failed";
    public const string Timeout = "timeout";
    public const string ConnectionFailed = "connection-failed";
    public const string Failed = "failed";
}

public sealed class SdtdDiagnosticsProbe(SdtdApiClient client)
{
    public async Task<SdtdDiagnosticsReport> CheckReadinessAsync()
    {
        ArgumentNullException.ThrowIfNull(client);

        var checks = new List<SdtdDiagnosticsCheck>
        {
            await RunCheckAsync("server-info", async () =>
            {
                var info = await client.GetServerInfoAsync();
                return $"server info responded for {info.GameName.Value}";
            }),
            await RunCheckAsync("stats", async () =>
            {
                var stats = await client.GetStatsAsync();
                return $"stats responded with {stats.Players} players";
            }),
            await RunCheckAsync("allowed-commands", async () =>
            {
                var commands = await client.GetAllowedCommandsAsync();
                return $"allowed commands responded with {commands.Commands.Count} commands";
            })
        };

        return new SdtdDiagnosticsReport(checks);
    }

    private static async Task<SdtdDiagnosticsCheck> RunCheckAsync(string name, Func<Task<string>> action)
    {
        try
        {
            return new SdtdDiagnosticsCheck(name, SdtdDiagnosticsStatus.Ready, await action());
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return new SdtdDiagnosticsCheck(name, SdtdDiagnosticsStatus.AuthFailed, ex.Message);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is not null)
        {
            return new SdtdDiagnosticsCheck(name, SdtdDiagnosticsStatus.HttpFailed, ex.Message);
        }
        catch (HttpRequestException ex)
        {
            return new SdtdDiagnosticsCheck(name, SdtdDiagnosticsStatus.ConnectionFailed, ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            return new SdtdDiagnosticsCheck(name, SdtdDiagnosticsStatus.Timeout, ex.Message);
        }
        catch (JsonException ex)
        {
            return new SdtdDiagnosticsCheck(name, SdtdDiagnosticsStatus.ParseFailed, ex.Message);
        }
        catch (Exception ex) when (ex.Message.StartsWith("Expected JSON response", StringComparison.Ordinal))
        {
            return new SdtdDiagnosticsCheck(name, SdtdDiagnosticsStatus.ParseFailed, ex.Message);
        }
        catch (Exception ex)
        {
            return new SdtdDiagnosticsCheck(name, SdtdDiagnosticsStatus.Failed, ex.Message);
        }
    }
}
