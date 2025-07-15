namespace PolyhydraGames.SdtdSharp.Models
{
    public record LandClaimsResponse(
        int Claimsize,
        List<ClaimOwner> Claimowners
    );
}