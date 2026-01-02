using ChessLib;
using ChessLib.Common;
using ChessLib.Pieces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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