namespace PolyhydraGames.SdtdSharp.Models
{
    public record ClaimOwner(
        string Steamid,
        bool Claimactive,
        string Playername,
        List<Position> Claims = default
    );
}