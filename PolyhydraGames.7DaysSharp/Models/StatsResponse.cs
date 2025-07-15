namespace PolyhydraGames.SdtdSharp.Models
{
    public record StatsResponse(
        GameTime Gametime,
        int Players,
        int Hostiles,
        int Animals
    );
}