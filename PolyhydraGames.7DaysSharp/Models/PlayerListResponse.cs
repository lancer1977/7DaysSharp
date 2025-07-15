namespace PolyhydraGames.SdtdSharp.Models
{
    public record PlayerListResponse(
        int Total,
        int TotalUnfiltered,
        int FirstResult,
        List<PlayerNotOnline> Players
    );
}