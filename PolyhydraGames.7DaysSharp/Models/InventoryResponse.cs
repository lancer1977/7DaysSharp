namespace PolyhydraGames.SdtdSharp.Models
{
    public record InventoryResponse(
        string Playername,
        string Userid,
        string Steamid,
        List<object> Bag = default
    );
}