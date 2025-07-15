namespace PolyhydraGames.SdtdSharp.Models
{
    public record PlayerNotOnline(
        string Steamid,
        long Entityid,
        string Ip,
        string Name,
        bool Online,
        Position Position = default,
        double Totalplaytime = 0,
        string Lastonline = "",
        double Ping = 0,
        bool Banned = false
    );
}