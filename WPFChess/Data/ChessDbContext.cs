using Microsoft.EntityFrameworkCore;
using ChessWPF.Models;

namespace ChessWPF.Data
{
    /// <summary>
    /// Database context for chess game records
    /// </summary>
    public class ChessDbContext : DbContext
    {
        public DbSet<GameRecord> GameRecords { get; set; }
        public DbSet<HistoricalGame> HistoricalGames { get; set; }
        public DbSet<ParsedFile> ParsedFiles { get; set; }

        public ChessDbContext()
        {
        }

        public ChessDbContext(DbContextOptions<ChessDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=chess_games.sqlite");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GameRecord>(entity =>
            {
                entity.ToTable("GameRecords");
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.WhitePlayer);
                entity.HasIndex(e => e.BlackPlayer);
            });

            modelBuilder.Entity<HistoricalGame>(entity =>
            {
                entity.ToTable("HistoricalGames");
                entity.HasIndex(e => e.PlayedAt);
                entity.HasIndex(e => e.WhitePlayer);
                entity.HasIndex(e => e.BlackPlayer);
            });

            modelBuilder.Entity<ParsedFile>(entity =>
            {
                entity.ToTable("ParsedFiles");
                entity.HasIndex(e => e.FileName).IsUnique();
            });
        }
    }
}
