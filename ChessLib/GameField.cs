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
        /// Checks if cell is attacked
        /// </summary>
        /// <param name="pieces"></param>
        /// <param name="cell"></param>
        /// <param name="gameField"></param>
        public bool GetAtackStatus(List<IPiece> pieces, Position cell, string[,] gameField)
        {
            var AllPossibleMoves = new List<Position>();
            foreach (var piece in pieces)
            {
                AllPossibleMoves.AddRange(piece.AvailableMoves(gameField));
                AllPossibleMoves.AddRange(piece.AvailableKills(gameField));
            }
            this[cell.X, cell.Y].IsAtacked = AllPossibleMoves.Contains(cell);
            return AllPossibleMoves.Contains(cell);
        }
        public static bool GetCheckStatusAfterMove(List<IPiece> pieces, IPiece chosenPiece, Position destinationCell)
        {
            pieces.Find(piece => piece == chosenPiece).Position = destinationCell;

            Cell[,] board = new Cell[8, 8];

            for(int i = 0; i < 8; i++)
            {
                for(int j = 0; j < 8; j++)
                {
                    board[i, j] = new Cell();
                }
            }

            foreach(var piece in pieces)
            {
                board[piece.Position.X, piece.Position.Y].Piece = piece;
            }
            
            // Calculate attack status for all cells
            var gameFieldString = GetStringFromGameField(board);
            var allPieces = pieces.ToList();
            var enemyPieces = allPieces.Where(p => p.Color != chosenPiece.Color).ToList();
            
            // Update attack status for all cells
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var pos = new Position(i, j);
                    var allPossibleMoves = new List<Position>();
                    foreach (var piece in enemyPieces)
                    {
                        allPossibleMoves.AddRange(piece.AvailableMoves(gameFieldString));
                        allPossibleMoves.AddRange(piece.AvailableKills(gameFieldString));
                    }
                    board[i, j].IsAtacked = allPossibleMoves.Contains(pos);
                }
            }
            
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[i,j].IsAtacked && board[i, j].Piece is King)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Checks if our king will be in check after current move
        /// </summary>
        /// <param name="Pieces">All pieces</param>
        /// <param name="ChosenPiece">Piece chosen for move</param>
        /// <param name="DestinationCell">Destination cell</param>
        /// <param name="CurrentPlayer">Current player</param>
        /// <returns></returns>
        public bool GetCheckStatusAfterMove(List<IPiece> Pieces, IPiece ChosenPiece, Position DestinationCell, Player CurrentPlayer)
        {
            List<IPiece> CopiedPieces = HardCloningOfTheList(Pieces);

            var CopiedChosenPiece = ChosenPiece.Clone();

            Cell[,] FieldAfterMove = GetFieldAfterMove(ChosenPiece, DestinationCell, CopiedPieces, CopiedChosenPiece);

            var EnemyPieces = CopiedPieces.Where(piece => piece.Color != CurrentPlayer.Color).ToList();

            var MyPieces = CopiedPieces.Where(piece => piece.Color == CurrentPlayer.Color).ToList();

            King MyKing = (King)MyPieces.Where(piece => piece is King).ToList()[0];

            var AllAvalaibleAttacksOfEnemies = new List<(string, List<Position>)>();

            foreach (var EnemyPiece in EnemyPieces)
            {
                AllAvalaibleAttacksOfEnemies.Add((EnemyPiece.ToString(), EnemyPiece.AvailableKills(GameField.GetStringFromGameField(FieldAfterMove))));
            }

            return AllAvalaibleAttacksOfEnemies.Select(x => x.Item2).ToList().SelectMany(a => a).ToList().Contains(MyKing.Position);
        }

        private Cell[,] GetFieldAfterMove(IPiece ChosenPiece, Position DestinationCell, List<IPiece> CopiedPieces, object CopiedChosenPiece)
        {
            Cell[,] CloneOfTheField = HardCloningOfTheField();

            CloneOfTheField[ChosenPiece.Position.X, ChosenPiece.Position.Y].Piece = null;

            if (CloneOfTheField[DestinationCell.X, DestinationCell.Y].IsFilled)
            {
                CopiedPieces.Find(piece => piece.Position == DestinationCell).IsDead = true;

                List<IPiece> updatedPieces = new List<IPiece>();

                foreach (var piece in CopiedPieces)
                {
                    if (!piece.IsDead)
                    {
                        updatedPieces.Add(piece);
                    }
                }
                CopiedPieces = updatedPieces;
            }

            CopiedPieces.Find(piece => piece.Position == ((IPiece)CopiedChosenPiece).Position).ChangePosition(DestinationCell);

            CloneOfTheField[DestinationCell.X, DestinationCell.Y].Piece = (IPiece)CopiedChosenPiece;

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
                        IsAtacked = Field[i, j].IsAtacked
                    };

                    if (Field[i, j].IsFilled)
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
        /// Копируем элементы списка фигур в новый список (Элементы создаются новые и они никак не связаны с оригиналами)
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

        public static string[,] GetStringFromGameField(Cell[,] cells)
        {
            string[,] StringFromGameField = new string[8, 8];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (!cells[i, j].IsFilled)
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
        //public string GetFENFromGamefield()
        //{
        //    return Fen.GetFenFromTheGameField();
        //}

        /// <summary>
        /// Checks if cell is free
        /// </summary>
        /// <param name="Cell"></param>
        /// <param name="gameField"></param>
        /// <returns></returns>
        public bool IsCellFree(Position Cell, string[,] gameField)
        {
            return gameField[Cell.X, Cell.Y] == " ";
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
                    if (this[i, j].IsAtacked && this[i, j].Piece is King)
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
                    this[i, j].IsAtacked = false;
                }
            }


            foreach (var piece in pieces)
            {
                int i = piece.Position.X;
                int j = piece.Position.Y;

                this[i, j].Piece = piece;

                GetAtackStatus(enemyPices, piece.Position, gameFiled);
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
