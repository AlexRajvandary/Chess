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
                
                // Verify tables exist by trying to query them
                try
                {
                    var count = context.GameRecords.Count();
                }
                catch (Exception tableEx)
                {
                    // Table doesn't exist - this shouldn't happen after EnsureCreated()
                    // But if it does, try to create it again
                    System.Diagnostics.Debug.WriteLine($"GameRecords table verification failed: {tableEx.Message}");
                    
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

                // Verify HistoricalGames table exists
                try
                {
                    var count = context.HistoricalGames.Count();
                }
                catch (Exception tableEx)
                {
                    // HistoricalGames table doesn't exist - recreate database to include it
                    System.Diagnostics.Debug.WriteLine($"HistoricalGames table verification failed: {tableEx.Message}");
                    
                    try
                    {
                        // If HistoricalGames table doesn't exist, we need to recreate the database
                        // or apply a migration. For simplicity, we'll recreate if it's missing.
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                    }
                    catch
                    {
                        MessageBox.Show($"Не удалось создать таблицу HistoricalGames. Ошибка: {tableEx.Message}", 
                            "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                // Verify ParsedFiles table exists
                try
                {
                    var count = context.ParsedFiles.Count();
                }
                catch (Exception tableEx)
                {
                    // ParsedFiles table doesn't exist - recreate database to include it
                    System.Diagnostics.Debug.WriteLine($"ParsedFiles table verification failed: {tableEx.Message}");
                    
                    try
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                    }
                    catch
                    {
                        MessageBox.Show($"Не удалось создать таблицу ParsedFiles. Ошибка: {tableEx.Message}", 
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

        public static List<HistoricalGame> GetAllHistoricalGames()
        {
            try
            {
                using var context = new ChessDbContext();
                return [.. context.HistoricalGames.OrderByDescending(g => g.PlayedAt ?? DateTime.MinValue)];
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке исторических партий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }

        /// <summary>
        /// Gets historical games with pagination
        /// </summary>
        public static (List<HistoricalGame> Games, int TotalCount) GetHistoricalGamesPaginated(int pageNumber, int pageSize)
        {
            try
            {
                using var context = new ChessDbContext();
                var query = context.HistoricalGames.OrderByDescending(g => g.PlayedAt ?? DateTime.MinValue);
                var totalCount = query.Count();
                var games = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                return (games, totalCount);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке исторических партий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return ([], 0);
            }
        }

        /// <summary>
        /// Gets the size of the database file in bytes
        /// </summary>
        public static long GetDatabaseSize()
        {
            try
            {
                var dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chess_games.sqlite");
                if (System.IO.File.Exists(dbPath))
                {
                    var fileInfo = new System.IO.FileInfo(dbPath);
                    return fileInfo.Length;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public static HistoricalGame GetHistoricalGameById(int id)
        {
            try
            {
                using var context = new ChessDbContext();
                return context.HistoricalGames.FirstOrDefault(g => g.Id == id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке исторической партии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Checks if a file has already been parsed
        /// </summary>
        public static bool IsFileParsed(string fileName)
        {
            try
            {
                using var context = new ChessDbContext();
                return context.ParsedFiles.Any(pf => pf.FileName == fileName);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Splits a PGN file content into individual game strings
        /// Games are separated by two or more consecutive empty lines, or by a new [Event header
        /// </summary>
        private static List<string> SplitPgnIntoGames(string pgnContent)
        {
            var games = new List<string>();
            var lines = pgnContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var currentGame = new System.Text.StringBuilder();
            bool inGame = false;
            bool inHeaders = true;
            int consecutiveEmptyLines = 0;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                
                // Start of a new game (header line starting with [Event)
                if (trimmed.StartsWith("[Event"))
                {
                    // If we already have a game, save it
                    if (inGame && currentGame.Length > 0)
                    {
                        var gameText = currentGame.ToString().Trim();
                        if (gameText.Length > 0 && HasGameResult(gameText))
                        {
                            games.Add(gameText);
                        }
                        currentGame.Clear();
                    }
                    inGame = true;
                    inHeaders = true;
                    consecutiveEmptyLines = 0;
                    currentGame.AppendLine(line);
                }
                // Other header lines (if we're in a game)
                else if (trimmed.StartsWith("[") && trimmed.EndsWith("]") && inGame)
                {
                    consecutiveEmptyLines = 0;
                    currentGame.AppendLine(line);
                }
                // Empty line
                else if (string.IsNullOrWhiteSpace(trimmed))
                {
                    if (inGame)
                    {
                        consecutiveEmptyLines++;
                        if (inHeaders)
                        {
                            // First empty line after headers - marks start of moves section
                            inHeaders = false;
                            currentGame.AppendLine();
                        }
                        else if (consecutiveEmptyLines >= 2)
                        {
                            // Two or more consecutive empty lines - end of current game
                            var gameText = currentGame.ToString().Trim();
                            if (gameText.Length > 0 && HasGameResult(gameText))
                            {
                                games.Add(gameText);
                            }
                            currentGame.Clear();
                            inGame = false;
                            inHeaders = true;
                            consecutiveEmptyLines = 0;
                        }
                    }
                }
                // Regular content line (moves or other headers)
                else if (inGame)
                {
                    consecutiveEmptyLines = 0;
                    currentGame.AppendLine(line);
                }
            }

            // Add last game if exists
            if (inGame && currentGame.Length > 0)
            {
                var lastGame = currentGame.ToString().Trim();
                if (lastGame.Length > 0 && HasGameResult(lastGame))
                {
                    games.Add(lastGame);
                }
            }

            return games;
        }

        /// <summary>
        /// Checks if game text contains a result marker at the end
        /// </summary>
        private static bool HasGameResult(string gameText)
        {
            // Check for result at the end of the game (1-0, 0-1, 1/2-1/2, *)
            var trimmed = gameText.Trim();
            return System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"\s+(1-0|0-1|1/2-1/2|\*)\s*$");
        }

        /// <summary>
        /// Parses PGN date string (can be in format "YYYY.MM.DD", "YYYY.??.??", "YYYY.MM.??", etc.)
        /// </summary>
        private static DateTime? ParsePgnDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
                return null;

            dateStr = dateStr.Trim();

            // Try standard DateTime parsing first
            if (DateTime.TryParse(dateStr, out var parsedDate))
            {
                return parsedDate;
            }

            // Handle PGN date formats like "1880.??.??", "1904.??.??", "YYYY.MM.??", "YYYY.??.DD"
            var parts = dateStr.Split('.');
            if (parts.Length >= 1)
            {
                // Try to parse year
                if (int.TryParse(parts[0], out int year) && year >= 1000 && year <= 9999)
                {
                    int month = 1;
                    int day = 1;

                    // Try to parse month if available
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int parsedMonth) && parsedMonth >= 1 && parsedMonth <= 12)
                    {
                        month = parsedMonth;
                    }

                    // Try to parse day if available
                    if (parts.Length >= 3 && int.TryParse(parts[2], out int parsedDay) && parsedDay >= 1 && parsedDay <= 31)
                    {
                        day = parsedDay;
                    }

                    try
                    {
                        return new DateTime(year, month, day);
                    }
                    catch
                    {
                        // If invalid date (e.g., Feb 30), use first day of year
                        try
                        {
                            return new DateTime(year, 1, 1);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Imports games from a PGN file with progress reporting
        /// </summary>
        public static ImportResult ImportPgnFile(string filePath, System.Action<int, int, int> progressCallback = null)
        {
            var result = new ImportResult();
            
            try
            {
                var fileName = System.IO.Path.GetFileName(filePath);
                
                // Check if file already parsed
                if (IsFileParsed(fileName))
                {
                    result.Success = false;
                    result.Message = $"Файл {fileName} уже был импортирован ранее.";
                    return result;
                }

                // Read file content
                if (!System.IO.File.Exists(filePath))
                {
                    result.Success = false;
                    result.Message = $"Файл не найден: {filePath}";
                    return result;
                }

                var fileContent = System.IO.File.ReadAllText(filePath);
                
                // Split into individual games
                var gameStrings = SplitPgnIntoGames(fileContent);
                result.TotalGames = gameStrings.Count;

                if (gameStrings.Count == 0)
                {
                    result.Success = false;
                    result.Message = "В файле не найдено ни одной игры.";
                    return result;
                }

                // Report initial progress with total count
                progressCallback?.Invoke(0, 0, gameStrings.Count);

                using var context = new ChessDbContext();
                var pgnService = new PgnService();
                var importedCount = 0;
                var skippedCount = 0;

                for (int i = 0; i < gameStrings.Count; i++)
                {
                    try
                    {
                        var gamePgn = gameStrings[i];
                        var headers = PgnService.ParsePgnHeaders(gamePgn);
                        var moves = PgnService.ParsePgnMoves(gamePgn);

                        if (moves.Count == 0)
                        {
                            skippedCount++;
                            continue;
                        }

                        // Create a game and replay moves to get final FEN
                        var game = new Game();
                        var initialFen = headers.ContainsKey("FEN") ? headers["FEN"] : "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
                        
                        // Note: For now, we always start from standard position
                        // Custom FEN positions would require additional implementation

                        // Replay moves
                        var pgnMoveParser = new PgnMoveParser();
                        foreach (var moveNotation in moves)
                        {
                            var moveInfo = PgnMoveParser.ParseMove(moveNotation, game);
                            if (moveInfo != null)
                            {
                                var moveResult = game.MakeMove(moveInfo.From, moveInfo.To);
                                if (!moveResult.IsValid)
                                {
                                    // Skip invalid moves
                                    break;
                                }
                            }
                        }

                        var finalFen = game.GetFen();
                        var whitePlayer = headers.ContainsKey("White") ? headers["White"] : "White";
                        var blackPlayer = headers.ContainsKey("Black") ? headers["Black"] : "Black";
                        var eventName = headers.ContainsKey("Event") ? headers["Event"] : null;
                        var site = headers.ContainsKey("Site") ? headers["Site"] : null;
                        var round = headers.ContainsKey("Round") ? headers["Round"] : null;
                        var resultStr = headers.ContainsKey("Result") ? headers["Result"] : "*";
                        
                        DateTime? playedAt = null;
                        if (headers.ContainsKey("Date"))
                        {
                            var dateStr = headers["Date"];
                            playedAt = ParsePgnDate(dateStr);
                        }

                        var historicalGame = new HistoricalGame
                        {
                            WhitePlayer = whitePlayer,
                            BlackPlayer = blackPlayer,
                            CreatedAt = DateTime.Now,
                            PlayedAt = playedAt,
                            PgnNotation = gamePgn,
                            Event = eventName,
                            Site = site,
                            Round = round,
                            InitialFen = initialFen,
                            FinalFen = finalFen,
                            MoveCount = moves.Count,
                            Result = resultStr
                        };

                        context.HistoricalGames.Add(historicalGame);
                        importedCount++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка при импорте игры {i + 1}: {ex.Message}");
                        skippedCount++;
                    }

                    // Report progress
                    var progress = (int)((i + 1) * 100.0 / gameStrings.Count);
                    progressCallback?.Invoke(progress, i + 1, gameStrings.Count);
                }

                // Save parsed file record
                var parsedFile = new ParsedFile
                {
                    FileName = fileName,
                    ParsedAt = DateTime.Now,
                    GamesImported = importedCount
                };
                context.ParsedFiles.Add(parsedFile);

                context.SaveChanges();

                result.Success = true;
                result.ImportedGames = importedCount;
                result.SkippedGames = skippedCount;
                result.Message = $"Импортировано игр: {importedCount}, пропущено: {skippedCount}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Ошибка при импорте файла: {ex.Message}";
            }

            return result;
        }
    }

    /// <summary>
    /// Result of PGN file import operation
    /// </summary>
    public class ImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalGames { get; set; }
        public int ImportedGames { get; set; }
        public int SkippedGames { get; set; }
    }
}