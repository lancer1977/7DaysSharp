namespace PolyhydraGames.SdtdSharp.Models
{
    public record OnlinePlayerResponse(
        string Steamid,
        string Userid,
        long Entityid,
        string Ip,
        string Name,
        bool Online,
        Position Position = default,
        double Level = 0,
        double Health = 0,
        double Stamina = 0,
        double Zombiekills = 0,
        double Playerkills = 0,
        double Playerdeaths = 0,
        double Score = 0,
        double Totalplaytime = 0,
        string Lastonline = "",
        double Ping = 0
    );
}