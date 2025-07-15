namespace PolyhydraGames.SdtdSharp.Models
{
    public record SdtdServerConfig( string Ip, int Port, bool? ForceHttps, string AdminUser, string AdminToken );
}