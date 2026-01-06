namespace PolyhydraGames.SdtdSharp;

public class SdtdBridge(SdtdApiClient config)
{
    public async Task<string> ListPlayers()
    {
        return await Execute("listplayers");
    }

    public async Task<string> KickPlayer(string playerIdOrName, string reason = "")
    {
        return await Execute($"kick {playerIdOrName} \"{reason}\"");
    }

    public async Task<string> BanPlayer(string playerId, int durationMinutes, string reason)
    {
        return await Execute($"ban add {playerId} {durationMinutes} \"{reason}\"");
    }

    public async Task<string> UnbanPlayer(string playerId)
    {
        return await Execute($"ban remove {playerId}");
    }

    public async Task<string> Say(string message)
    {
        return await Execute($"say \"{message}\"");
    }

    public async Task<string> GetTime()
    {
        return await Execute("gettime");
    }

    public async Task<string> SetTime(string time)
    {
        return await Execute($"settime {time}");
    }

    public async Task<string> Weather(string condition)
    {
        return await Execute($"weather {condition}");
    }

    public async Task<string> Teleport(string playerId, float x, float y, float z)
    {
        return await Execute($"teleport {playerId} {x} {y} {z}");
    }

    public async Task<string> SpawnEntity(string playerId, string entityId)
    {
        return await Execute($"spawnentity {playerId} {entityId}");
    }

    public async Task<string> GiveQuest(string questName)
    {
        return await Execute($"givequest {questName}");
    }

    public async Task<string> Shutdown()
    {
        return await Execute("shutdown");
    }

    public async Task<string> Buff(string buffId) => await Execute($"buff {buffId}");

    public async Task<string> Debuff(string buffId) => await Execute($"debuff {buffId}");

    public async Task<string> SpawnScouts() => await Execute("spawnscouts");

    public async Task<string> SpawnHorde() => await Execute("spawnhorde");

    private async Task<string> Execute(string command)
    {
        try
        {
            var result = await config.ExecuteConsoleCommandAsync(command);
            return result.Result;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}


