using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPFView
{
    public enum State
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
