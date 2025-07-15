using PolyhydraGames.SdtdSharp.Models;
using System.Text.Json;
using System.Web;

namespace PolyhydraGames.SdtdSharp;

public class SdtdApiClient
{
    private SdtdServerConfig server;
    public SdtdApiClient(SdtdServerConfig server, HttpClient client)
    {
        this.server = server ?? throw new ArgumentNullException(nameof(server));
        this._client = client ?? throw new ArgumentNullException(nameof(client));
    }
    private readonly HttpClient _client;

    private string GetBaseUrl()
    {
        var scheme = "http";
        var needsPort = true;

        if (server.ForceHttps == true)
            scheme = "https";
        else if (server.ForceHttps == false)
            scheme = "http";
        else if (server.Port == 443)
        {
            scheme = "https";
            needsPort = false;
        }
        else if (server.Port == 80)
        {
            scheme = "http";
            needsPort = false;
        }

        return needsPort
            ? $"{scheme}://{server.Ip}:{server.Port}"
            : $"{scheme}://{server.Ip}";
    }

    private async Task<T> FetchJsonAsync<T>(string path, Dictionary<string, object?> queryParams)
    {
        var baseUrl = GetBaseUrl();
        var query = HttpUtility.ParseQueryString(string.Empty);

        foreach (var kv in queryParams)
        {
            if (kv.Value != null)
                query[kv.Key] = kv.Value.ToString();
        }

        var uri = $"{baseUrl}{path}?{query}";
        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        if (queryParams.TryGetValue("adminuser", out var adminUser) && adminUser != null)
            request.Headers.Add("X-SDTD-API-TOKENNAME", adminUser.ToString());

        if (queryParams.TryGetValue("admintoken", out var adminToken) && adminToken != null)
            request.Headers.Add("X-SDTD-API-SECRET", adminToken.ToString());

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var contentType = response.Content.Headers.ContentType?.MediaType;
        var content = await response.Content.ReadAsStringAsync();

        if (contentType == "application/json")
        {
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        throw new Exception("Expected JSON response but got: " + contentType);
    }

    public Task<StatsResponse> GetStatsAsync() =>
        FetchJsonAsync<StatsResponse>( "/api/getstats", AuthParams(server));

    public Task<List<OnlinePlayerResponse>> GetOnlinePlayersAsync() =>
        FetchJsonAsync<List<OnlinePlayerResponse>>( "/api/getlayersonline", AuthParams(server));

    public Task<AllowedCommands> GetAllowedCommandsAsync() =>
        FetchJsonAsync<AllowedCommands>( "/api/getallowedcommands", AuthParams(server));

    public Task<CommandResponse> ExecuteConsoleCommandAsync(string command)
    {
        var qs = AuthParams(server);
        qs["command"] = command;
        return FetchJsonAsync<CommandResponse>( "/api/executeconsolecommand", qs);
    }

    public Task<List<EntityLocation>> GetAnimalsLocationAsync() =>
        FetchJsonAsync<List<EntityLocation>>( "/api/getanimalslocation", AuthParams(server));

    public Task<List<EntityLocation>> GetHostileLocationAsync() =>
        FetchJsonAsync<List<EntityLocation>>( "/api/gethostilelocation", AuthParams(server));

    public Task<LandClaimsResponse> GetLandClaimsAsync(string? steamId = null)
    {
        var qs = AuthParams(server);
        if (!string.IsNullOrWhiteSpace(steamId))
            qs["steamid"] = steamId;
        return FetchJsonAsync<LandClaimsResponse>( "/api/getlandclaims", qs);
    }

    public Task<InventoryResponse> GetPlayerInventoryAsync(string steamId)
    {
        var qs = AuthParams(server);
        if (System.Text.RegularExpressions.Regex.IsMatch(steamId, @"^\d{17}$"))
        {
            qs["steamid"] = steamId;
            qs["userid"] = $"Steam_{steamId}";
        }
        else
        {
            qs["userid"] = steamId;
        }

        return FetchJsonAsync<InventoryResponse>( "/api/getplayerinventory", qs);
    }

    public Task<List<InventoryResponse>> GetPlayerInventoriesAsync() =>
        FetchJsonAsync<List<InventoryResponse>>( "/api/getplayerinventories", AuthParams(server));

    public Task<PlayerListResponse> GetPlayerListAsync(int? rowsPerPage = null, int? page = null)
    {
        var qs = AuthParams(server);
        if (rowsPerPage.HasValue) qs["rowsperpage"] = rowsPerPage.Value;
        if (page.HasValue) qs["page"] = page.Value;
        return FetchJsonAsync<PlayerListResponse>( "/api/getplayerlist", qs);
    }

    public Task<List<PlayerLocation>> GetPlayersLocationAsync(bool offline)
    {
        var qs = AuthParams(server);
        qs["offline"] = offline;
        return FetchJsonAsync<List<PlayerLocation>>( "/api/getplayerslocation", qs);
    }

    public Task<GetServerInfo> GetServerInfoAsync() =>
        FetchJsonAsync<GetServerInfo>( "/api/getserverinfo", AuthParams(server));

    public Task<GetWebUIUpdatesResponse> GetWebUIUpdatesAsync(int? latestLine = null)
    {
        var qs = AuthParams(server);
        if (latestLine.HasValue) qs["latestLine"] = latestLine;
        return FetchJsonAsync<GetWebUIUpdatesResponse>( "/api/getwebuiupdates", qs);
    }

    public Task<GetLog> GetLogAsync(int? firstLine = null, int count = 50)
    {
        var qs = AuthParams(server);
        if (firstLine.HasValue) qs["firstLine"] = firstLine.Value;
        qs["count"] = count;
        return FetchJsonAsync<GetLog>( "/api/getlog", qs);
    }

    private Dictionary<string, object?> AuthParams(SdtdServerConfig config) => new()
        {
            { "adminuser", config.AdminUser },
            { "admintoken", config.AdminToken }
        };
}