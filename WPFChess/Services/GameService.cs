using ChessLib;
using ChessLib.Common;
using ChessLib.Pieces;
using ChessLib.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChessWPF.Services
{
    public class GameService : IGameService
    {
        private readonly IGameEngine _gameEngine;

        public GameService(IGameEngine gameEngine)
        {
            _gameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));
            LogInfo("GameService initialized");
        }

        public GameService() : this(new GameEngine())
        {
        }

        public MoveResult MakeMove(ChessLib.Common.Position from, ChessLib.Common.Position to)
        {
            LogInfo($"MakeMove: {from} -> {to}");
            var result = _gameEngine.MakeMove(from, to);
            
            if (result.IsValid)
            {
                LogInfo($"Move successful: {result.MoveType}, Check: {result.IsCheck}, Checkmate: {result.IsCheckmate}");
            }
            else
            {
                LogWarning($"Move failed: {result.ErrorMessage ?? "Unknown error"}");
            }
            
            return result;
        }

        public IGameState GetState()
        {
            var state = _gameEngine.GetState();
            LogDebug($"GetState: CurrentPlayer={state.CurrentPlayerColor}, Check={state.IsCheck}, Checkmate={state.IsCheckmate}, GameOver={state.IsGameOver}");
            return state;
        }

        public IReadOnlyList<ChessLib.Common.Position> GetValidMoves(ChessLib.Common.Position position)
        {
            var moves = _gameEngine.GetValidMoves(position);
            LogDebug($"GetValidMoves({position}): Found {moves.Count} valid moves");
            return moves;
        }

        public string GetFen()
        {
            var fen = _gameEngine.GetFen();
            LogDebug($"GetFen: {fen}");
            return fen;
        }

        public string GetMoveHistory()
        {
            var history = _gameEngine.GetMoveHistory();
            LogDebug($"GetMoveHistory: {history?.Length ?? 0} characters");
            return history;
        }

        public void StartNewGame()
        {
            LogInfo("StartNewGame: Starting a new game");
            _gameEngine.StartNewGame();
            LogInfo("StartNewGame: New game started successfully");
        }

        public void EndGameByTime(PieceColor losingColor)
        {
            LogInfo($"EndGameByTime: Game ended, losing color: {losingColor}");
            _gameEngine.EndGameByTime(losingColor);
        }

        public void RestoreFromFen(string fen)
        {
            LogInfo($"RestoreFromFen: Restoring game from FEN");
            _gameEngine.RestoreFromFen(fen);
            LogInfo("RestoreFromFen: Game restored successfully");
        }

        public void RestoreFromMoveHistory(IEnumerable<string> moves)
        {
            LogInfo($"RestoreFromMoveHistory: Restoring game from {moves?.Count() ?? 0} moves");
            _gameEngine.RestoreFromMoveHistory(moves);
            LogInfo("RestoreFromMoveHistory: Game restored successfully");
        }

        public void RestoreFromPgn(string pgn)
        {
            LogInfo("RestoreFromPgn: Restoring game from PGN");
            _gameEngine.RestoreFromPgn(pgn);
            LogInfo("RestoreFromPgn: Game restored successfully");
        }

        public ParsedMove ParseMove(string moveNotation)
        {
            LogDebug($"ParseMove: Parsing move notation '{moveNotation}'");
            var parsedMove = AlgebraicMoveParser.ParseMove(moveNotation, this);
            if (parsedMove != null)
            {
                LogDebug($"ParseMove: Parsed successfully - {parsedMove.From} -> {parsedMove.To}");
            }
            else
            {
                LogWarning($"ParseMove: Failed to parse move notation '{moveNotation}'");
            }
            return parsedMove;
        }

        public string GeneratePgn(string whitePlayer = "White", string blackPlayer = "Black", 
            string eventName = null, string site = null, string round = null, 
            string result = null, PieceColor? timeLoser = null)
        {
            LogDebug("GeneratePgn: Generating PGN notation");
            // Pass _gameEngine explicitly since PgnService expects IGameEngine
            var pgn = ChessLib.Services.PgnService.GeneratePgn(
                _gameEngine, 
                whitePlayer, 
                blackPlayer, 
                eventName, 
                site, 
                round, 
                result, 
                timeLoser);
            LogDebug($"GeneratePgn: Generated PGN ({pgn?.Length ?? 0} characters)");
            return pgn;
        }

        public Dictionary<string, string> ParsePgnHeaders(string pgn)
        {
            LogDebug("ParsePgnHeaders: Parsing PGN headers");
            var headers = PgnParser.ParseHeaders(pgn);
            LogDebug($"ParsePgnHeaders: Parsed {headers.Count} headers");
            return headers;
        }

        public List<string> ParsePgnMoves(string pgn)
        {
            LogDebug("ParsePgnMoves: Parsing PGN moves");
            var moves = PgnParser.ParseMoves(pgn);
            LogDebug($"ParsePgnMoves: Parsed {moves.Count} moves");
            return moves;
        }

        #region Logging

        private void LogInfo(string message)
        {
            Debug.WriteLine($"[GameService] [INFO] {DateTime.Now:HH:mm:ss.fff} - {message}");
        }

        private void LogWarning(string message)
        {
            Debug.WriteLine($"[GameService] [WARN] {DateTime.Now:HH:mm:ss.fff} - {message}");
        }

        private void LogDebug(string message)
        {
#if DEBUG
            Debug.WriteLine($"[GameService] [DEBUG] {DateTime.Now:HH:mm:ss.fff} - {message}");
#endif
        }

        #endregion
    }
}