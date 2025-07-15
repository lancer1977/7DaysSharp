namespace PolyhydraGames.SdtdSharp.Models
{
    public record PlayerLocation(
        string Steamid,
        string Userid,
        string Name,
        bool Online,
        Position Position = default
    );
}