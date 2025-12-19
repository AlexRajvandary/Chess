namespace ChessWPF.Models
{
    /// <summary>
    /// UI state for a chess cell - represents piece type and color for display
    /// </summary>
    public enum CellUIState
    {
        Empty,       // пусто
        WhiteKing,   // король
        WhiteQueen,  // ферзь
        WhiteRook,   // ладья
        WhiteKnight, // конь
        WhiteBishop, // слон
        WhitePawn,   // пешка
        BlackKing,
        BlackQueen,
        BlackRook,
        BlackKnight,
        BlackBishop,
        BlackPawn
    }
}