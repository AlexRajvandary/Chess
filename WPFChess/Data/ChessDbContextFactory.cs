using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChessWPF.Data
{
    /// <summary>
    /// Factory for creating DbContext during design time (for migrations)
    /// </summary>
    public class ChessDbContextFactory : IDesignTimeDbContextFactory<ChessDbContext>
    {
        public ChessDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ChessDbContext>();
            optionsBuilder.UseSqlite("Data Source=chess_games.sqlite");

            return new ChessDbContext(optionsBuilder.Options);
        }
    }
}
