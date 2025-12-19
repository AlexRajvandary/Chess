using System.Collections.Generic;
using System.Linq;

namespace ChessLib
{
    /// <summary>
    /// Main game class - provides clean API for chess game logic
    /// </summary>
    public class Game
    {
        private readonly MoveValidator moveValidator;
        private readonly MoveExecutor moveExecutor;

        public GameField GameField { get; private set; }

        /// <summary>
        /// Current player index (0 - white, 1 - black)
        /// </summary>
        public int CurrentPlayer { get; private set; }

        /// <summary>
        /// Pieces on the board
        /// </summary>
        public List<IPiece> Pieces { get; private set; }

        /// <summary>
        /// Players
        /// </summary>
        public List<Player> Players { get; private set; }

        /// <summary>
        /// Game over flag
        /// </summary>
        public bool IsGameOver { get; private set; }

        /// <summary>
        /// Move history
        /// </summary>
        public List<MoveNotation> MoveHistory { get; private set; }

        /// <summary>
        /// Gets current player color
        /// </summary>
        public PieceColor CurrentPlayerColor => CurrentPlayer % 2 == 0 ? PieceColor.White : PieceColor.Black;

        /// <summary>
        /// Устанавливает начальные позиции фигурам
        /// </summary>
        /// <returns>Список фигур</returns>
        public static List<IPiece> GetPiecesStartPosition()
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
        public static string[,] GetGameField(List<IPiece> pieces)
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
        /// Initializes a new game
        /// </summary>
        public Game()
        {
            CurrentPlayer = 0;
            Pieces = new List<IPiece>();
            Pieces = GetPiecesStartPosition();
            GameField = new GameField();
            moveValidator = new MoveValidator(GameField);
            moveExecutor = new MoveExecutor(GameField);
            MoveHistory = new List<MoveNotation>();

            // Player with white pieces
            Player player1 = new(PieceColor.White, Pieces.Where(x => x.Color == PieceColor.White).ToList(), "user1");

            // Player with black pieces
            Player player2 = new Player(PieceColor.Black, Pieces.Where(x => x.Color == PieceColor.Black).ToList(), "user2");
            
            Players = new List<Player>
            {
                player1,
                player2
            };

            IsGameOver = false;
        }

        /// <summary>
        /// Makes a move from one position to another
        /// </summary>
        public MoveResult MakeMove(Position from, Position to)
        {
            if (IsGameOver)
                return MoveResult.Failure("Game is over");

            var piece = Pieces.FirstOrDefault(p => p.Position == from && !p.IsDead);
            if (piece == null)
                return MoveResult.Failure("No piece at source position");

            if (piece.Color != CurrentPlayerColor)
                return MoveResult.Failure("Not your turn");

            var gameFieldString = GetGameField(Pieces);

            // Validate move
            if (!moveValidator.IsValidMove(Pieces, piece, to, gameFieldString))
                return MoveResult.Failure("Invalid move");

            // Execute move
            var result = moveExecutor.ExecuteMove(Pieces, piece, to, gameFieldString);
            
            // Remove dead pieces
            moveExecutor.RemoveDeadPieces(Pieces);

            // Record move in history
            var moveNotation = new MoveNotation
            {
                From = from,
                To = to,
                Piece = piece,
                MoveType = result.MoveType,
                CapturedPiece = result.CapturedPiece,
                PlayerColor = CurrentPlayerColor,
                MoveNumber = (MoveHistory.Count / 2) + 1
            };

            // Update game field
            UpdateGameField();

            // Check for check/checkmate
            SwitchPlayer();
            var nextPlayerColor = CurrentPlayerColor;
            result.IsCheck = IsCheck(nextPlayerColor);
            result.IsCheckmate = result.IsCheck && IsCheckmate(nextPlayerColor);

            moveNotation.IsCheck = result.IsCheck;
            moveNotation.IsCheckmate = result.IsCheckmate;

            // Add to history
            MoveHistory.Add(moveNotation);

            if (result.IsCheckmate)
            {
                IsGameOver = true;
            }

            return result;
        }

        /// <summary>
        /// Gets all valid moves for a piece at the given position
        /// </summary>
        public List<Position> GetValidMoves(Position piecePosition)
        {
            var piece = Pieces.FirstOrDefault(p => p.Position == piecePosition && !p.IsDead);
            if (piece == null)
                return new List<Position>();

            if (piece.Color != CurrentPlayerColor)
                return new List<Position>();

            var gameFieldString = GetGameField(Pieces);
            var possibleMoves = piece.AvailableMoves(gameFieldString);
            var possibleKills = piece.AvailableKills(gameFieldString);
            var allPossibleMoves = possibleMoves.Concat(possibleKills).ToList();

            return moveValidator.FilterValidMoves(Pieces, piece, allPossibleMoves, gameFieldString);
        }

        /// <summary>
        /// Gets all valid moves for a specific piece
        /// </summary>
        public List<Position> GetValidMovesForPiece(IPiece piece)
        {
            if (piece == null || piece.IsDead)
                return new List<Position>();

            if (piece.Color != CurrentPlayerColor)
                return new List<Position>();

            var gameFieldString = GetGameField(Pieces);
            var possibleMoves = piece.AvailableMoves(gameFieldString);
            var possibleKills = piece.AvailableKills(gameFieldString);
            var allPossibleMoves = possibleMoves.Concat(possibleKills).ToList();

            return moveValidator.FilterValidMoves(Pieces, piece, allPossibleMoves, gameFieldString);
        }

        /// <summary>
        /// Checks if a move is valid
        /// </summary>
        public bool IsValidMove(Position from, Position to)
        {
            var piece = Pieces.FirstOrDefault(p => p.Position == from && !p.IsDead);
            if (piece == null)
                return false;

            if (piece.Color != CurrentPlayerColor)
                return false;

            var gameFieldString = GetGameField(Pieces);
            return moveValidator.IsValidMove(Pieces, piece, to, gameFieldString);
        }

        /// <summary>
        /// Checks if the specified color is in check
        /// </summary>
        public bool IsCheck(PieceColor color)
        {
            UpdateGameField();
            var gameFieldString = GetGameField(Pieces);
            var king = Pieces.FirstOrDefault(p => p is King && p.Color == color && !p.IsDead) as King;
            if (king == null)
                return false;

            var enemyPieces = Pieces.Where(p => p.Color != color && !p.IsDead).ToList();
            return GameField.GetAtackStatus(enemyPieces, king.Position, gameFieldString);
        }

        /// <summary>
        /// Checks if the specified color is in checkmate
        /// </summary>
        public bool IsCheckmate(PieceColor color)
        {
            if (!IsCheck(color))
                return false;

            // Check if any piece can make a valid move
            var playerPieces = Pieces.Where(p => p.Color == color && !p.IsDead).ToList();
            var gameFieldString = GetGameField(Pieces);

            foreach (var piece in playerPieces)
            {
                var possibleMoves = piece.AvailableMoves(gameFieldString);
                var possibleKills = piece.AvailableKills(gameFieldString);
                var allPossibleMoves = possibleMoves.Concat(possibleKills).ToList();

                foreach (var move in allPossibleMoves)
                {
                    if (moveValidator.IsValidMove(Pieces, piece, move, gameFieldString))
                        return false; // Found a valid move, not checkmate
                }
            }

            return true; // No valid moves found, it's checkmate
        }

        /// <summary>
        /// Gets the current game state
        /// </summary>
        public GameState GetState()
        {
            var state = new GameState
            {
                CurrentPlayerColor = CurrentPlayerColor,
                Pieces = Pieces.ToList(), // Create a copy
                BoardRepresentation = GetGameField(Pieces),
                IsCheck = IsCheck(CurrentPlayerColor),
                IsCheckmate = IsCheckmate(CurrentPlayerColor),
                IsGameOver = IsGameOver
            };

            if (state.IsCheckmate)
            {
                state.IsGameOver = true;
                state.GameOverReason = $"{CurrentPlayerColor} is checkmated";
            }

            return state;
        }

        /// <summary>
        /// Starts a new game
        /// </summary>
        public void StartNewGame()
        {
            CurrentPlayer = 0;
            Pieces = GetPiecesStartPosition();
            GameField = new GameField();
            MoveHistory = new List<MoveNotation>();
            IsGameOver = false;

            Players = new List<Player>
            {
                new Player(PieceColor.White, Pieces.Where(x => x.Color == PieceColor.White).ToList(), "user1"),
                new Player(PieceColor.Black, Pieces.Where(x => x.Color == PieceColor.Black).ToList(), "user2")
            };
        }

        /// <summary>
        /// Updates the game field state
        /// </summary>
        private void UpdateGameField()
        {
            var gameFieldString = GetGameField(Pieces);
            GameField.Update(Pieces, gameFieldString, CurrentPlayerColor);
        }

        /// <summary>
        /// Switches to the next player
        /// </summary>
        private void SwitchPlayer()
        {
            CurrentPlayer++;
            if (CurrentPlayer >= 2)
            {
                CurrentPlayer = 0;
            }
        }

        /// <summary>
        /// Gets FEN notation for current game state
        /// </summary>
        public string GetFen()
        {
            return Fen.GenerateFen(this);
        }

        /// <summary>
        /// Gets move history in algebraic notation
        /// </summary>
        public string GetMoveHistory()
        {
            return AlgebraicNotation.FormatMoveHistory(MoveHistory, Pieces);
        }
    }
}