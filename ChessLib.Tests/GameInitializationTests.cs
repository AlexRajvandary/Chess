using ChessLib;
using Xunit;

namespace ChessLib.Tests
{
    public class GameInitializationTests
    {
        [Fact]
        public void Game_ShouldInitializeWithCorrectStartingPosition()
        {
            // Arrange & Act
            var game = new Game();

            // Assert
            Assert.NotNull(game.Pieces);
            Assert.Equal(32, game.Pieces.Count); // 16 белых + 16 черных фигур
            Assert.False(game.IsGameOver);
            Assert.Equal(0, game.CurrentPlayer);
            Assert.Equal(PieceColor.White, game.CurrentPlayerColor);
        }

        [Fact]
        public void Game_WhiteShouldMoveFirst()
        {
            // Arrange & Act
            var game = new Game();

            // Assert
            Assert.Equal(PieceColor.White, game.CurrentPlayerColor);
            Assert.Equal(0, game.CurrentPlayer);
        }

        [Fact]
        public void Game_ShouldHaveCorrectPiecePositions()
        {
            // Arrange & Act
            var game = new Game();

            // Assert - проверяем ключевые фигуры
            var whiteKing = game.Pieces.FirstOrDefault(p => p is King && p.Color == PieceColor.White);
            var blackKing = game.Pieces.FirstOrDefault(p => p is King && p.Color == PieceColor.Black);
            
            Assert.NotNull(whiteKing);
            Assert.NotNull(blackKing);
            Assert.Equal(new Position(4, 0), whiteKing.Position);
            Assert.Equal(new Position(4, 7), blackKing.Position);
        }

        [Fact]
        public void Game_ShouldNotBeOverAtStart()
        {
            // Arrange & Act
            var game = new Game();

            // Assert
            Assert.False(game.IsGameOver);
            var state = game.GetState();
            Assert.False(state.IsCheck);
            Assert.False(state.IsCheckmate);
        }
    }
}



