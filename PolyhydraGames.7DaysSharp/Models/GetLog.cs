namespace PolyhydraGames.SdtdSharp.Models
{
    public record GetLog(int FirstLine, int LastLine, List<LogLine> entries);
}