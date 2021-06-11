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
        public bool GetAtackStatus(List<IPiece> pieces, (int, int) cell, string[,] gameField)
        {
            var AllPossibleMoves = new List<(int, int)>();
            foreach (var piece in pieces)
            {
                AllPossibleMoves.AddRange(piece.AvailableMoves(gameField));
                AllPossibleMoves.AddRange(piece.AvailableKills(gameField));
            }
            this[cell.Item1, cell.Item2].isAtacked = AllPossibleMoves.Contains(cell);
            return AllPossibleMoves.Contains(cell);
        }
        /// <summary>
        /// Узнаем будет ли шах нашему королю после текущего хода
        /// </summary>
        /// <param name="Pieces">Все фигуры</param>
        /// <param name="ChosenPiece">Фигура, выбранная для хода</param>
        /// <param name="DestinationCell">Клетка назначения</param>
        /// <param name="CurrentPlayer">Текущий игрок</param>
        /// <returns></returns>
        public bool GetCheckStatusAfterMove(List<IPiece> Pieces, IPiece ChosenPiece, (int, int) DestinationCell, Player CurrentPlayer)
        {
            List<IPiece> CopiedPieces = HardCloningOfTheList(Pieces);

            var CopiedChosenPiece = ChosenPiece.Clone();

            var EnemyPieces = CopiedPieces.Where(piece => piece.Color != CurrentPlayer.Color).ToList();

            var MyPieces = CopiedPieces.Where(piece => piece.Color == CurrentPlayer.Color).ToList();

            King MyKing = (King)MyPieces.Where(piece => piece is King).ToList()[0];

            Cell[,] FieldAfterMove = GetFieldAfterMove(ChosenPiece, DestinationCell, CopiedPieces, CopiedChosenPiece);

            var AllAvalaibleAttacksOfEnemies = new List<(string, List<(int, int)>)>();

            foreach (var EnemyPiece in EnemyPieces)
            {
                AllAvalaibleAttacksOfEnemies.Add((EnemyPiece.ToString(), EnemyPiece.AvailableKills(GetStringFromGameField(FieldAfterMove))));
            }

            return AllAvalaibleAttacksOfEnemies.Select(x => x.Item2).ToList().SelectMany(a => a).ToList().Contains(MyKing.Position);

        }

        private Cell[,] GetFieldAfterMove(IPiece ChosenPiece, (int, int) DestinationCell, List<IPiece> CopiedPieces, object CopiedChosenPiece)
        {
            Cell[,] CloneOfTheField = HardCloningOfTheField();

            CloneOfTheField[ChosenPiece.Position.Item1, ChosenPiece.Position.Item2].isFilled = false;

            CloneOfTheField[ChosenPiece.Position.Item1, ChosenPiece.Position.Item2].Piece = null;

            CopiedPieces.Find(piece => piece.Position == ((IPiece)CopiedChosenPiece).Position).ChangePosition(DestinationCell);

            CloneOfTheField[DestinationCell.Item1, DestinationCell.Item2].isFilled = true;

            CloneOfTheField[DestinationCell.Item1, DestinationCell.Item2].Piece = (IPiece)CopiedChosenPiece;

            return CloneOfTheField;
        }

        private Cell[,] HardCloningOfTheField()
        {
            Cell[,] FieldAfterMove = new Cell[8, 8];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    FieldAfterMove[i, j] = new Cell
                    {
                        isAtacked = Field[i, j].isAtacked,

                        isFilled = Field[i, j].isFilled
                    };

                    if (FieldAfterMove[i, j].isFilled)
                    {
                        FieldAfterMove[i, j].Piece = (IPiece)Field[i, j].Piece.Clone();
                    }
                    else
                    {
                        FieldAfterMove[i, j].Piece = null;
                    }

                }
            }

            return FieldAfterMove;
        }

        /// <summary>
        /// Копируем элементы списка в новый список (Элементы создаются новые и они никак не связаны с оригиналами)
        /// </summary>
        /// <param name="pieces">Исходный список фигур</param>
        /// <returns>Клон исходного списка</returns>
        private static List<IPiece> HardCloningOfTheList(List<IPiece> pieces)
        {
            var CopiedPieces = new List<IPiece>();
            foreach (var piece in pieces)
            {
                if (piece is Pawn)
                {
                    CopiedPieces.Add(new Pawn(piece.Color, piece.Position));
                }
                else if (piece is Rook)
                {
                    CopiedPieces.Add(new Rook(piece.Position, piece.Color));
                }
                else if (piece is Bishop)
                {
                    CopiedPieces.Add(new Bishop(piece.Position, piece.Color));
                }
                else if (piece is Knight)
                {
                    CopiedPieces.Add(new Knight(piece.Position, piece.Color));
                }
                else if (piece is Queen)
                {
                    CopiedPieces.Add(new Queen(piece.Color, piece.Position));
                }
                else if (piece is King)
                {
                    CopiedPieces.Add(new King(piece.Position, piece.Color));
                }
            }

            return CopiedPieces;
        }

        public string[,] GetStringFromGameField(Cell[,] cells)
        {
            string[,] StringFromGameField = new string[8, 8];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (cells[i, j].isFilled == false)
                    {
                        StringFromGameField[i, j] = " ";
                    }
                    else
                    {
                        StringFromGameField[i, j] = cells[i, j].Piece.ToString();
                    }
                }
            }
            return StringFromGameField;
        }

        /// <summary>
        /// Узнаем свободна ли клетка
        /// </summary>
        /// <param name="Cell"></param>
        /// <param name="gameField"></param>
        /// <returns></returns>
        public bool IsCellFree((int, int) Cell, string[,] gameField)
        {
            return gameField[Cell.Item1, Cell.Item2] == " ";
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

                        return true;

                    }
                }
            }
            return false;
        }

        public void Update(List<IPiece> pieces, string[,] gameFiled, PieceColor curretnPlayer)
        {
            var enemyPices = pieces.Where(piece => piece.Color != curretnPlayer).ToList();
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

                GetAtackStatus(enemyPices, (i, j), gameFiled);
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
