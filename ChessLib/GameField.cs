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
       
        public bool GetAtackStatus(List<IPiece> pieces, Position cell, string[,] gameField)
        {
            var AllPossibleMoves = new List<Position>();
            foreach (var piece in pieces)
            {
                if (piece != null && !piece.IsDead)
                {
                    AllPossibleMoves.AddRange(piece.AvailableMoves(gameField));
                    AllPossibleMoves.AddRange(piece.AvailableKills(gameField));
                }
            }
            bool isAttacked = AllPossibleMoves.Contains(cell);
           
            if (Field != null && cell.X >= 0 && cell.X < 8 && cell.Y >= 0 && cell.Y < 8)
            {
                this[cell.X, cell.Y].IsAtacked = isAttacked;
            }
            return isAttacked;
        }
        
        public static bool GetAtackStatusStatic(List<IPiece> pieces, Position cell, string[,] gameField)
        {
            var AllPossibleMoves = new List<Position>();
            foreach (var piece in pieces)
            {
                if (piece != null && !piece.IsDead)
                {
                    AllPossibleMoves.AddRange(piece.AvailableMoves(gameField));
                    AllPossibleMoves.AddRange(piece.AvailableKills(gameField));
                }
            }
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
            
            var gameFieldString = GetStringFromGameField(board);
            var allPieces = pieces.ToList();
            var enemyPieces = allPieces.Where(p => p.Color != chosenPiece.Color).ToList();
            
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
       
        public bool GetCheckStatusAfterMove(List<IPiece> Pieces, IPiece ChosenPiece, Position DestinationCell, Player CurrentPlayer)
        {
            List<IPiece> CopiedPieces = HardCloningOfTheList(Pieces);
            var CopiedChosenPiece = ChosenPiece.Clone();
            GetFieldAfterMove(ChosenPiece, DestinationCell, CopiedPieces, CopiedChosenPiece);
            var EnemyPieces = CopiedPieces.Where(piece => piece.Color != CurrentPlayer.Color && !piece.IsDead).ToList();
            var MyPieces = CopiedPieces.Where(piece => piece.Color == CurrentPlayer.Color && !piece.IsDead).ToList();
            King MyKing = (King)MyPieces.Where(piece => piece is King).ToList()[0];
            var gameFieldStringAfterMove = Game.GetGameField(CopiedPieces);
            var AllAvalaibleAttacksOfEnemies = new List<Position>();

            foreach (var EnemyPiece in EnemyPieces)
            {
                AllAvalaibleAttacksOfEnemies.AddRange(EnemyPiece.AvailableMoves(gameFieldStringAfterMove));
                AllAvalaibleAttacksOfEnemies.AddRange(EnemyPiece.AvailableKills(gameFieldStringAfterMove));
            }

            return AllAvalaibleAttacksOfEnemies.Contains(MyKing.Position);
        }

        private Cell[,] GetFieldAfterMove(IPiece ChosenPiece, Position DestinationCell, List<IPiece> CopiedPieces, object CopiedChosenPiece)
        {
            Cell[,] CloneOfTheField = HardCloningOfTheField();
            CloneOfTheField[ChosenPiece.Position.X, ChosenPiece.Position.Y].Piece = null;
            var pieceToMove = CopiedPieces.Find(piece => piece.Position == ChosenPiece.Position && piece.GetType() == ChosenPiece.GetType() && piece.Color == ChosenPiece.Color);
            pieceToMove?.ChangePosition(DestinationCell);
            if (CloneOfTheField[DestinationCell.X, DestinationCell.Y].IsFilled)
            {
                var capturedPiece = CopiedPieces.Find(piece => piece.Position == DestinationCell && piece.Color != ChosenPiece.Color);
                if (capturedPiece != null)
                {
                    capturedPiece.IsDead = true;
                }
            }

            CopiedPieces.RemoveAll(piece => piece.IsDead);

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

        public static bool IsCellFree(Position Cell, string[,] gameField)
        {
            return gameField[Cell.X, Cell.Y] == " ";
        }
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