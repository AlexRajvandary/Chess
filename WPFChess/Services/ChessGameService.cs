using System.Collections.Generic;
using System.Linq;
using ChessLib;

namespace ChessWPF.Services
{
    /// <summary>
    /// Service that provides a clean interface between WPF UI and ChessLib domain logic
    /// </summary>
    public class ChessGameService
    {
        private Game _game;

        public ChessGameService()
        {
            _game = new Game();
        }

        /// <summary>
        /// Gets current game instance
        /// </summary>
        public Game CurrentGame => _game;

        /// <summary>
        /// Starts a new game
        /// </summary>
        public void StartNewGame()
        {
            _game = new Game();
        }

        /// <summary>
        /// Gets board state snapshot for UI
        /// </summary>
        public BoardStateSnapshot GetBoardState()
        {
            var state = _game.GetState();
            var snapshot = new BoardStateSnapshot();

            // Initialize empty board
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    snapshot.Cells[i, j] = null;
                }
            }

            // Fill with pieces
            foreach (var piece in state.Pieces.Where(p => !p.IsDead))
            {
                snapshot.Cells[piece.Position.X, piece.Position.Y] = new PieceInfo
                {
                    Piece = piece,
                    Color = piece.Color,
                    Type = GetPieceType(piece)
                };
            }

            snapshot.CurrentPlayerColor = state.CurrentPlayerColor;
            snapshot.IsCheck = state.IsCheck;
            snapshot.IsCheckmate = state.IsCheckmate;
            snapshot.IsGameOver = state.IsGameOver;

            return snapshot;
        }

        /// <summary>
        /// Gets valid moves for a piece at the given position
        /// </summary>
        public List<ChessLib.Position> GetValidMoves(ChessLib.Position position)
        {
            return _game.GetValidMoves(position);
        }

        /// <summary>
        /// Makes a move and returns the result
        /// </summary>
        public MoveResult MakeMove(ChessLib.Position from, ChessLib.Position to)
        {
            return _game.MakeMove(from, to);
        }

        /// <summary>
        /// Gets FEN notation for current game state
        /// </summary>
        public string GetFen()
        {
            return _game.GetFen();
        }

        /// <summary>
        /// Gets move history in algebraic notation
        /// </summary>
        public string GetMoveHistory()
        {
            return _game.GetMoveHistory();
        }

        /// <summary>
        /// Ends the game due to time expiration
        /// </summary>
        public void EndGameByTime(PieceColor losingColor)
        {
            _game.EndGameByTime(losingColor);
        }

        /// <summary>
        /// Converts IPiece to PieceType enum
        /// </summary>
        private PieceType GetPieceType(IPiece piece)
        {
            return piece switch
            {
                Pawn => PieceType.Pawn,
                Rook => PieceType.Rook,
                Knight => PieceType.Knight,
                Bishop => PieceType.Bishop,
                Queen => PieceType.Queen,
                King => PieceType.King,
                _ => PieceType.Pawn
            };
        }
    }

    /// <summary>
    /// Snapshot of board state for UI consumption
    /// </summary>
    public class BoardStateSnapshot
    {
        public PieceInfo[,] Cells { get; set; } = new PieceInfo[8, 8];
        public PieceColor CurrentPlayerColor { get; set; }
        public bool IsCheck { get; set; }
        public bool IsCheckmate { get; set; }
        public bool IsGameOver { get; set; }
    }

    /// <summary>
    /// Information about a piece on the board
    /// </summary>
    public class PieceInfo
    {
        public IPiece Piece { get; set; }
        public PieceColor Color { get; set; }
        public PieceType Type { get; set; }
    }

    /// <summary>
    /// Piece type enumeration
    /// </summary>
    public enum PieceType
    {
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
    }
}

