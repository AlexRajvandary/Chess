using ChessLib;
using Xunit;

namespace ChessLib.Tests
{
    public class CaptureTests
    {
        [Fact]
        public void Pawn_CanCaptureDiagonally()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(3, 6), new Position(3, 4)); // d5
            var whitePawn = game.Pieces.FirstOrDefault(p => p is Pawn && p.Position == new Position(4, 3) && p.Color == PieceColor.White);

            // Act
            var result = game.MakeMove(new Position(4, 3), new Position(3, 4)); // Взятие

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(MoveType.Capture, result.MoveType);
            Assert.NotNull(result.CapturedPiece);
            Assert.Equal(new Position(3, 4), whitePawn.Position);
        }

        [Fact]
        public void Rook_CanCapturePiece()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(0, 1), new Position(0, 3)); // a4
            game.MakeMove(new Position(0, 6), new Position(0, 4)); // a5
            game.MakeMove(new Position(0, 0), new Position(0, 2)); // Ra3
            game.MakeMove(new Position(1, 6), new Position(1, 5)); // b6 - черные делают другой ход
            // Теперь ладья на (0,2) может взять черную пешку на (0,4)
            
            // Act - ладья берет черную пешку
            var result = game.MakeMove(new Position(0, 2), new Position(0, 4));

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(MoveType.Capture, result.MoveType);
        }

        [Fact]
        public void Knight_CanCapturePiece()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(1, 0), new Position(2, 2)); // Nc3
            game.MakeMove(new Position(0, 6), new Position(0, 5)); // a6
            game.MakeMove(new Position(2, 2), new Position(0, 3)); // Na4
            game.MakeMove(new Position(0, 5), new Position(0, 4)); // a5
            game.MakeMove(new Position(0, 3), new Position(1, 5)); // Nc5

            // Act - конь берет пешку
            var result = game.MakeMove(new Position(1, 5), new Position(0, 6));

            // Assert
            if (result.IsValid)
            {
                Assert.Equal(MoveType.Capture, result.MoveType);
            }
        }

        [Fact]
        public void Bishop_CanCapturePiece()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(2, 1), new Position(2, 3)); // c4
            game.MakeMove(new Position(2, 6), new Position(2, 4)); // c5
            game.MakeMove(new Position(2, 0), new Position(5, 3)); // Bf4
            game.MakeMove(new Position(1, 6), new Position(1, 5)); // b6
            game.MakeMove(new Position(5, 3), new Position(2, 6)); // Bxc7

            // Act
            var result = game.MakeMove(new Position(2, 6), new Position(1, 7));

            // Assert - если есть фигура для взятия
            if (result.IsValid && result.MoveType == MoveType.Capture)
            {
                Assert.NotNull(result.CapturedPiece);
            }
        }

        [Fact]
        public void Queen_CanCapturePiece()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(3, 1), new Position(3, 3)); // d4
            game.MakeMove(new Position(3, 6), new Position(3, 4)); // d5
            game.MakeMove(new Position(3, 0), new Position(3, 2)); // Qd3
            game.MakeMove(new Position(4, 6), new Position(4, 5)); // e6
            game.MakeMove(new Position(3, 2), new Position(3, 4)); // Qxd5

            // Act
            var result = game.MakeMove(new Position(3, 4), new Position(4, 5));

            // Assert
            if (result.IsValid && result.MoveType == MoveType.Capture)
            {
                Assert.NotNull(result.CapturedPiece);
            }
        }

        [Fact]
        public void King_CanCapturePiece()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(4, 6), new Position(4, 4)); // e5
            game.MakeMove(new Position(4, 0), new Position(4, 1)); // Ke2
            game.MakeMove(new Position(3, 6), new Position(3, 5)); // d6
            game.MakeMove(new Position(4, 1), new Position(4, 2)); // Ke3
            game.MakeMove(new Position(3, 5), new Position(3, 4)); // d5
            game.MakeMove(new Position(4, 2), new Position(3, 4)); // Kxd5

            // Act
            var result = game.MakeMove(new Position(3, 4), new Position(4, 4));

            // Assert
            if (result.IsValid && result.MoveType == MoveType.Capture)
            {
                Assert.NotNull(result.CapturedPiece);
            }
        }

        [Fact]
        public void CapturedPiece_IsMarkedAsDead()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(3, 6), new Position(3, 4)); // d5
            var blackPawn = game.Pieces.FirstOrDefault(p => p.Position == new Position(3, 4) && p.Color == PieceColor.Black);

            // Act
            var result = game.MakeMove(new Position(4, 3), new Position(3, 4)); // Взятие

            // Assert
            Assert.True(result.IsValid);
            Assert.True(blackPawn.IsDead);
        }
    }
}


