using ChessLib;
using ChessLib.Common;
using ChessLib.Pieces;
using ChessLib.Services;
using System.Collections.Generic;

namespace ChessWPF.Services
{
    /// <summary>
    /// Service interface for game operations in WPF application
    /// Extends IGameEngine to allow using library services that require IGameEngine
    /// </summary>
    public interface IGameService : IGameEngine
    {
        // All methods from IGameEngine are inherited:
        // MoveResult MakeMove(Position from, Position to);
        // IReadOnlyList<Position> GetValidMoves(Position position);
        // IGameState GetState();
        // string GetFen();
        // string GetMoveHistory();
        // void StartNewGame();
        // void EndGameByTime(PieceColor losingColor);
        // void RestoreFromFen(string fen);
        // void RestoreFromMoveHistory(IEnumerable<string> moves);
        // void RestoreFromPgn(string pgn);

        /// <summary>
        /// Parses a move in algebraic notation
        /// </summary>
        ParsedMove ParseMove(string moveNotation);

        /// <summary>
        /// Generates PGN notation for the current game
        /// </summary>
        string GeneratePgn(string whitePlayer = "White", string blackPlayer = "Black", 
            string eventName = null, string site = null, string round = null, 
            string result = null, PieceColor? timeLoser = null);

        /// <summary>
        /// Parses PGN headers from a PGN string
        /// </summary>
        Dictionary<string, string> ParsePgnHeaders(string pgn);

        /// <summary>
        /// Parses PGN moves from a PGN string
        /// </summary>
        List<string> ParsePgnMoves(string pgn);
    }
}