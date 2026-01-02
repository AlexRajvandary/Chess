using ChessLib.Pieces;
using System;
using System.Text;

namespace ChessLib.Services
{
    /// <summary>
    /// Service for generating PGN (Portable Game Notation) format
    /// Works with IGameEngine instead of Game directly
    /// </summary>
    public class PgnService
    {
        private const string ClassicStartPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        /// <summary>
        /// Generates PGN notation from game engine
        /// </summary>
        /// <param name="gameEngine">Game engine to generate PGN from</param>
        /// <param name="whitePlayer">White player name</param>
        /// <param name="blackPlayer">Black player name</param>
        /// <param name="eventName">Event name</param>
        /// <param name="site">Site name</param>
        /// <param name="round">Round number</param>
        /// <param name="result">Game result (1-0, 0-1, 1/2-1/2, *) - if null, will be determined from game state</param>
        /// <param name="timeLoser">Color of player who lost by time (if applicable)</param>
        /// <returns>PGN string</returns>
        public static string GeneratePgn(
            IGameEngine gameEngine,
            string whitePlayer = "White",
            string blackPlayer = "Black",
            string eventName = null,
            string site = null,
            string round = null,
            string result = null,
            PieceColor? timeLoser = null)
        {
            ArgumentNullException.ThrowIfNull(gameEngine);

            var pgn = new StringBuilder();
            var state = gameEngine.GetState();

            // Headers
            pgn.AppendLine($"[Event \"{eventName ?? "Casual Game"}\"]");
            pgn.AppendLine($"[Site \"{site ?? "?"}\"]");
            pgn.AppendLine($"[Date \"{DateTime.Now:yyyy.MM.dd}\"]");
            pgn.AppendLine($"[Round \"{round ?? "?"}\"]");
            pgn.AppendLine($"[White \"{whitePlayer}\"]");
            pgn.AppendLine($"[Black \"{blackPlayer}\"]");

            // Determine result if not provided
            if (string.IsNullOrEmpty(result))
            {
                result = DetermineGameResult(state, timeLoser);
            }
            pgn.AppendLine($"[Result \"{result}\"]");

            // FEN header if not standard starting position
            var currentFen = gameEngine.GetFen();
            if (currentFen != ClassicStartPositionFen)
            {
                pgn.AppendLine($"[FEN \"{currentFen}\"]");
            }

            pgn.AppendLine();

            // Moves in algebraic notation
            var moveHistory = gameEngine.GetMoveHistory();
            if (!string.IsNullOrEmpty(moveHistory))
            {
                pgn.Append(moveHistory);
            }

            pgn.Append($" {result}");

            return pgn.ToString();
        }

        /// <summary>
        /// Determines game result from game state
        /// </summary>
        private static string DetermineGameResult(IGameState state, PieceColor? timeLoser)
        {
            if (!state.IsGameOver)
            {
                return "*";
            }

            // Check if game ended by time
            if (timeLoser.HasValue)
            {
                // Player who lost by time - opposite player wins
                return timeLoser.Value == PieceColor.White ? "0-1" : "1-0";
            }

            // Check if checkmate
            if (state.IsCheckmate)
            {
                // The player who is checkmated loses
                // If current player is in checkmate, they lost
                // So the previous player (opposite color) won
                var winner = state.CurrentPlayerColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
                return winner == PieceColor.White ? "1-0" : "0-1";
            }

            // Otherwise, draw
            return "1/2-1/2";
        }
    }
}