#nullable disable
namespace ChessWPF.Services
{
    /// <summary>
    /// Result of PGN file import operation
    /// </summary>
    public class ImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalGames { get; set; }
        public int ImportedGames { get; set; }
        public int SkippedGames { get; set; }
    }
}