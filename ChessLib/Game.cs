using System.Collections.Generic;
using System.Linq;

namespace ChessLib
{
    public class Game
    {
        public GameField GameField { get; set; }

        /// <summary>
        /// Текущий игрок (0 - белые, 1 - черные)
        /// </summary>
        public int CurrentPlayer { get; set; }

        /// <summary>
        /// Фигуры на доске
        /// </summary>
        public List<IPiece> Pieces { get; set; }

        /// <summary>
        /// Игроки
        /// </summary>
        public List<Player> players { get; set; }

        /// <summary>
        /// Флаг окончания игры
        /// </summary>
        public bool IsGameOver { get; set; }

        /// <summary>
        /// Устанавливает начальные позиции фигурам
        /// </summary>
        /// <returns>Список фигур</returns>
        public List<IPiece> GetPiecesStartPosition()
        {
            var Pieces = new List<IPiece>();
            //Create pawns
            for (int i = 0; i < 8; i++)
            {
                var wPawn = new Pawn(PieceColor.White, new Position(i, 1));
                var bPawn = new Pawn(PieceColor.Black, new Position(i, 6));
                Pieces.Add(wPawn);
                Pieces.Add(bPawn);
            }
            //create bishops
            Pieces.Add(new Bishop(new Position(2, 0), PieceColor.White));
            Pieces.Add(new Bishop(new Position(5, 0), PieceColor.White));
            Pieces.Add(new Bishop(new Position(2, 7), PieceColor.Black));
            Pieces.Add(new Bishop(new Position(5, 7), PieceColor.Black));

            //create rooks
            Pieces.Add(new Rook(new Position(0, 0), PieceColor.White));
            Pieces.Add(new Rook(new Position(7, 0), PieceColor.White));
            Pieces.Add(new Rook(new Position(0, 7), PieceColor.Black));
            Pieces.Add(new Rook(new Position(7, 7), PieceColor.Black));

            //create knights
            Pieces.Add(new Knight(new Position(1, 0), PieceColor.White));
            Pieces.Add(new Knight(new Position(6, 0), PieceColor.White));
            Pieces.Add(new Knight(new Position(1, 7), PieceColor.Black));
            Pieces.Add(new Knight(new Position(6, 7), PieceColor.Black));

            //create queens
            Pieces.Add(new Queen(PieceColor.Black, new Position(3, 7)));
            Pieces.Add(new Queen(PieceColor.White, new Position(3, 0)));

            //create kings
            Pieces.Add(new King(new Position(4, 0), PieceColor.White));
            Pieces.Add(new King(new Position(4, 7), PieceColor.Black));

            return Pieces;
        }

        /// <summary>
        /// Получаем строковое представление игровой доски из позиций фигур
        /// </summary>
        /// <param name="pieces">Список фигур</param>
        /// <returns>Строковое представление игровой доски</returns>
        public string[,] GetGameField(List<IPiece> pieces)
        {
            string[,] GameField = new string[8, 8];
            foreach (var piece in pieces)
            {
                GameField[piece.Position.X, piece.Position.Y] = piece.Color == PieceColor.White ? piece.ToString().ToUpper() : piece.ToString();
            }
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (GameField[i, j] == null)
                    {
                        GameField[i, j] = " ";
                    }
                }
            }
            return GameField;
        }

        /// <summary>
        /// Method for WPF version: checks if current move is an attack on a piece, if yes, assigns attacked status
        /// </summary>
        /// <param name="PiecePosition">Position of selected piece</param>
        /// <param name="AttackedPosition">Move</param>
        /// <param name="gameField">Game field</param>
        /// <param name="pieces">Pieces</param>
        public void CheckIfPieceWasKilled(Position PiecePosition, Position AttackedPosition, string[,] gameField, List<IPiece> pieces)
        {
            //Check if desired move is an attempt to capture a piece (if among pieces there is one already at position where current player wants to move, then current player captures that piece)
            if (gameField[AttackedPosition.X, AttackedPosition.Y] != "")
            {
                pieces.Find(x => x.Position == AttackedPosition).IsDead = true;
                pieces.Find(x => x.Position == PiecePosition).Position = AttackedPosition;
            }
        }

        /// <summary>
        /// Убираем убитые фигуры
        /// </summary>
        /// <param name="pieces"></param>
        public void RemoveDeadPieces(List<IPiece> pieces)
        {
            pieces.RemoveAll(x => x.IsDead == true);
        }

        /// <summary>
        /// Инициализирует новую игру
        /// </summary>
        public Game()
        {
            CurrentPlayer = 0;
            Pieces = new List<IPiece>();
            Pieces = GetPiecesStartPosition();
            GameField = new GameField();

            // Игрок с белыми фигурами
            Player player1 = new Player(PieceColor.White, Pieces.Where(x => x.Color == PieceColor.White).ToList(), "user1");

            // Игрок с черными фигурами
            Player player2 = new Player(PieceColor.Black, Pieces.Where(x => x.Color == PieceColor.Black).ToList(), "user2");
            
            players = new List<Player>
            {
                player1,
                player2
            };

            IsGameOver = false;
        }
    }
}
