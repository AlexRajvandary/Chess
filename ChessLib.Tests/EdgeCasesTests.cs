using ChessLib;
using Xunit;

namespace ChessLib.Tests
{
    public class EdgeCasesTests
    {
        [Fact]
        public void CannotMoveNonExistentPiece()
        {
            // Arrange
            var game = new Game();

            // Act
            var result = game.MakeMove(new Position(3, 3), new Position(3, 4)); // Пустая клетка

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("No piece at source position", result.ErrorMessage);
        }

        [Fact]
        public void CannotMoveDeadPiece()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(3, 6), new Position(3, 4)); // d5
            var capturedPawn = game.Pieces.FirstOrDefault(p => p.Position == new Position(3, 4) && p.Color == PieceColor.Black);
            game.MakeMove(new Position(4, 3), new Position(3, 4)); // Взятие
            Assert.True(capturedPawn.IsDead);

            // Act - пытаемся ходить мертвой фигурой
            var result = game.MakeMove(new Position(3, 4), new Position(3, 5));

            // Assert - это должна быть белая пешка, которая взяла, не мертвая фигура
            // Уточним тест - проверим, что мертвая фигура не может ходить
            var deadPiece = game.Pieces.FirstOrDefault(p => p.IsDead);
            if (deadPiece != null)
            {
                var deadPieceMove = game.MakeMove(deadPiece.Position, new Position(deadPiece.Position.X, deadPiece.Position.Y + 1));
                Assert.False(deadPieceMove.IsValid);
            }
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
        public void PlayerSwitches_AfterEachMove()
        {
            // Arrange
            var game = new Game();
            Assert.Equal(PieceColor.White, game.CurrentPlayerColor);

            // Act
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // Белые

            // Assert
            Assert.Equal(PieceColor.Black, game.CurrentPlayerColor);

            // Act
            game.MakeMove(new Position(4, 6), new Position(4, 4)); // Черные

            // Assert
            Assert.Equal(PieceColor.White, game.CurrentPlayerColor);
        }

        [Fact]
        public void CannotMoveToSameSquare()
        {
            // Arrange
            var game = new Game();

            // Act
            var result = game.MakeMove(new Position(4, 1), new Position(4, 1)); // В ту же клетку

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void GetState_ReturnsCorrectGameState()
        {
            // Arrange
            var game = new Game();

            // Act
            var state = game.GetState();

            // Assert
            Assert.NotNull(state);
            Assert.Equal(PieceColor.White, state.CurrentPlayerColor);
            Assert.False(state.IsCheck);
            Assert.False(state.IsCheckmate);
            Assert.False(state.IsGameOver);
            Assert.NotNull(state.Pieces);
            Assert.NotNull(state.BoardRepresentation);
        }

        [Fact]
        public void StartNewGame_ResetsGameState()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(4, 1), new Position(4, 3));
            game.MakeMove(new Position(4, 6), new Position(4, 4));

            // Act
            game.StartNewGame();

            // Assert
            Assert.Equal(0, game.CurrentPlayer);
            Assert.Equal(PieceColor.White, game.CurrentPlayerColor);
            Assert.Empty(game.MoveHistory);
            Assert.False(game.IsGameOver);
            Assert.Equal(32, game.Pieces.Count);
        }
    }
}



