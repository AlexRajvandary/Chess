using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using ChessWPF.Data;
using ChessWPF.Models;
using ChessLib;

#nullable disable
namespace ChessWPF.Services
{
    public class GameStorageService
    {
        private readonly PgnService _pgnService;

        public GameStorageService()
        {
            _pgnService = new PgnService();
            EnsureDatabaseCreated();
        }

        private void EnsureDatabaseCreated()
        {
            try
            {
                using var context = new ChessDbContext();
                
                // Force creation of database and tables
                // EnsureCreated() creates the database and all tables based on the model
                var created = context.Database.EnsureCreated();
                
                // Verify table exists by trying to query it
                try
                {
                    var count = context.GameRecords.Count();
                }
                catch (Exception tableEx)
                {
                    // Table doesn't exist - this shouldn't happen after EnsureCreated()
                    // But if it does, try to create it again
                    System.Diagnostics.Debug.WriteLine($"Table verification failed: {tableEx.Message}");
                    
                    // Delete database and recreate
                    try
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                    }
                    catch
                    {
                        // If that fails, show error
                        MessageBox.Show($"Не удалось создать таблицу GameRecords. Ошибка: {tableEx.Message}", 
                            "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                
                // After ensuring database and tables exist, try to apply migrations
                // This will add any new columns from migrations
                try
                {
                    context.Database.Migrate();
                }
                catch
                {
                    // If migrations fail (e.g., no migration history), that's OK
                    // EnsureCreated() already created the basic structure
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                MessageBox.Show($"Ошибка при инициализации базы данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public GameRecord SaveGame(Game game, 
                                   string whitePlayer = "White",
                                   string blackPlayer = "Black",
                                   string eventName = null,
                                   string site = null,
                                   string round = null)
        {
            try
            {
                var pgn = _pgnService.GeneratePgn(game, whitePlayer, blackPlayer, eventName, site, round);
                // Get FEN after all moves (final position)
                var finalFen = game.GetFen();
                // Initial FEN is the starting position (standard or custom)
                var initialFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

                var gameRecord = new GameRecord
                {
                    WhitePlayer = whitePlayer,
                    BlackPlayer = blackPlayer,
                    CreatedAt = DateTime.Now,
                    PlayedAt = DateTime.Now,
                    PgnNotation = pgn,
                    Event = eventName,
                    Site = site,
                    Round = round,
                    InitialFen = initialFen,
                    FinalFen = finalFen,
                    MoveCount = game.MoveHistory?.Count ?? 0,
                    Result = game.IsGameOver 
                        ? (game.MoveHistory?.LastOrDefault()?.IsCheckmate == true
                            ? (game.MoveHistory.Last().PlayerColor == PieceColor.White ? "1-0" : "0-1")
                            : "1/2-1/2")
                        : "*"
                };

                using var context = new ChessDbContext();
                context.GameRecords.Add(gameRecord);
                context.SaveChanges();

                return gameRecord;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении партии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static List<GameRecord> GetAllGames()
        {
            try
            {
                using var context = new ChessDbContext();
                return [.. context.GameRecords.OrderByDescending(g => g.CreatedAt)];
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке партий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        public static GameRecord GetGameById(int id)
        {
            try
            {
                using var context = new ChessDbContext();
                return context.GameRecords.FirstOrDefault(g => g.Id == id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке партии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static bool DeleteGame(int id)
        {
            try
            {
                using var context = new ChessDbContext();
                var game = context.GameRecords.FirstOrDefault(g => g.Id == id);
                if (game != null)
                {
                    context.GameRecords.Remove(game);
                    context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении партии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}