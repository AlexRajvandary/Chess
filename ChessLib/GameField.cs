using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessLib
{
    public class GameField
    {
        Cell[,] Field { get; }

        public Cell this[int i, int j]
        {
            get
            {
                return Field[i, j];
            }

        }
        /// <summary>
        /// Узнаем атакована ли клетка
        /// </summary>
        /// <param name="pieces"></param>
        /// <param name="cell"></param>
        /// <param name="gameField"></param>
        void GetAtackStatus(List<IPiece> pieces, (int, int) cell, string[,] gameField)
        {
            var AllPossibleMoves = new List<(int, int)>();
            foreach (var piece in pieces)
            {
                AllPossibleMoves.AddRange(piece.AvailableMoves(gameField));
                AllPossibleMoves.AddRange(piece.AvailableKills(gameField));
            }
            this[cell.Item1, cell.Item2].isAtacked = AllPossibleMoves.Contains(cell);
            Console.WriteLine(this[cell.Item1, cell.Item2].isAtacked + " " + $"{cell.Item2 + 1}" + " " + "abcdefgh"[cell.Item1]);
        }
        /// <summary>
        /// Если клетка атакована и на ней король, то шах
        /// </summary>
        /// <returns></returns>
        public bool IsCheck()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (this[i, j].isAtacked && this[i, j].Piece is King)
                    {
                        Console.WriteLine($"{this[i, j].isAtacked}    {i + 1} {"abcdefgh"[j]}");

                        return true;

                    }
                }
            }
            return false;
        }
        public void Update(List<IPiece> pieces, string[,] gameFiled, PieceColor curretnPlayer)
        {
            var oppositePices = pieces.Where(piece => piece.Color != curretnPlayer).ToList();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    this[i, j].Piece = null;
                    this[i, j].isFilled = false;
                    this[i, j].isAtacked = false;
                }
            }


            foreach (var piece in pieces)
            {
                int i = piece.Position.Item1;
                int j = piece.Position.Item2;

                this[i, j].isFilled = true;
                this[i, j].Piece = piece;

                GetAtackStatus(oppositePices, (i, j), gameFiled);
            }
        }

        public GameField()
        {
            Field = new Cell[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Field[i, j] = new Cell();
                }
            }

        }


    }
}
