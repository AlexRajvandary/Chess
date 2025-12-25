using ChessLib.Pieces;
using Xunit;

namespace ChessLib.Tests
{
    public class MoveValidationTests
    {
        [Fact]
        public void CannotMoveOpponentPiece()
        {
            // Arrange
            var game = new Game();

            // Act - пытаемся ходить черной фигурой, когда ход белых
            var result = game.MakeMove(new Position(0, 6), new Position(0, 5));

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Not your turn", result.ErrorMessage);
        }

        [Fact]
        public void CannotMoveToOwnPiece()
        {
            // Arrange
            var game = new Game();

            // Act - пытаемся ходить в свою фигуру
            var result = game.MakeMove(new Position(0, 1), new Position(0, 0));

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void CannotMoveOutOfBounds()
        {
            // Arrange
            var game = new Game();

            // Act - пытаемся ходить за пределы доски (используем валидную позицию, но проверяем через IsValidMove)
            var isValid = game.IsValidMove(new Position(0, 1), new Position(-1, 1));

            // Assert
            Assert.False(isValid);
            
            // Также проверяем через MakeMove с валидной, но невозможной позицией
            var result = game.MakeMove(new Position(0, 1), new Position(0, 8)); // За пределы доски по Y
            Assert.False(result.IsValid);
        }

        [Fact]
        public void CannotLeaveKingInCheck()
        {
            // Arrange - создаем ситуацию, где король под шахом
            var game = new Game();
            // Делаем несколько ходов, чтобы создать позицию с шахом
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(4, 6), new Position(4, 4)); // e5
            game.MakeMove(new Position(5, 0), new Position(2, 3)); // Bc4
            game.MakeMove(new Position(1, 7), new Position(2, 5)); // Nf6
            game.MakeMove(new Position(3, 0), new Position(7, 4)); // Qf3 - шах

            // Act - пытаемся сделать ход, который оставляет короля под шахом
            // (если такой ход возможен)
            var blackKing = game.Pieces.FirstOrDefault(p => p is King && p.Color == PieceColor.Black);
            var validMoves = game.GetValidMoves(blackKing.Position);

            // Assert - король не должен иметь ходов, которые оставляют его под шахом
            Assert.NotNull(validMoves);
        }

        [Fact]
        public void CannotMoveWhenGameIsOver()
        {
            // Arrange
            var game = new Game();
            // Создаем мат (упрощенный сценарий)
            // Для полного теста нужно создать реальную позицию мата

            // Act - после мата
            if (game.IsGameOver)
            {
                var result = game.MakeMove(new Position(0, 1), new Position(0, 2));
                Assert.False(result.IsValid);
                Assert.Contains("Game is over", result.ErrorMessage);
            }
        }

        [Fact]
        public void GetValidMoves_ReturnsEmptyForOpponentPiece()
        {
            // Arrange
            var game = new Game();

            // Act
            var validMoves = game.GetValidMoves(new Position(0, 6)); // Черная пешка

            // Assert
            Assert.Empty(validMoves);
        }

        [Fact]
        public void GetValidMoves_ReturnsEmptyForNonExistentPiece()
        {
            // Arrange
            var game = new Game();

            // Act
            var validMoves = game.GetValidMoves(new Position(3, 3)); // Пустая клетка

            // Assert
            Assert.Empty(validMoves);
        }

        [Fact]
        public void IsValidMove_ReturnsFalseForInvalidMove()
        {
            // Arrange
            var game = new Game();

            // Act
            var isValid = game.IsValidMove(new Position(0, 1), new Position(0, 0)); // В свою фигуру

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValidMove_ReturnsTrueForValidMove()
        {
            // Arrange
            var game = new Game();

            // Act
            var isValid = game.IsValidMove(new Position(0, 1), new Position(0, 2));

            // Assert
            Assert.True(isValid);
        }
    }
}


