using ChessLib;
using Xunit;

namespace ChessLib.Tests
{
    public class CastlingTests
    {
        [Fact]
        public void White_CanPerformShortCastling()
        {
            // Arrange
            var game = new Game();
            // Освобождаем путь для рокировки
            game.MakeMove(new Position(5, 1), new Position(5, 3)); // f3
            game.MakeMove(new Position(5, 6), new Position(5, 4)); // f6
            game.MakeMove(new Position(6, 1), new Position(6, 3)); // g3
            game.MakeMove(new Position(6, 6), new Position(6, 4)); // g6
            // Не двигаем короля и ладью - они должны остаться на начальных позициях для рокировки!

            // Act - короткая рокировка белых
            var result = game.MakeMove(new Position(4, 0), new Position(6, 0));

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(MoveType.Castle, result.MoveType);
            var king = game.Pieces.FirstOrDefault(p => p is King && p.Color == PieceColor.White) as King;
            var rook = game.Pieces.FirstOrDefault(p => p is Rook && p.Position == new Position(5, 0) && p.Color == PieceColor.White) as Rook;
            Assert.Equal(new Position(6, 0), king.Position);
            Assert.Equal(new Position(5, 0), rook.Position);
            Assert.True(king.IsMoved);
            Assert.True(rook.IsMoved);
        }

        [Fact]
        public void White_CanPerformLongCastling()
        {
            // Arrange
            var game = new Game();
            // Освобождаем путь для длинной рокировки
            game.MakeMove(new Position(1, 1), new Position(1, 3)); // b3
            game.MakeMove(new Position(1, 6), new Position(1, 4)); // b6
            game.MakeMove(new Position(2, 0), new Position(1, 2)); // Bb2
            game.MakeMove(new Position(2, 7), new Position(1, 5)); // Bb7
            game.MakeMove(new Position(3, 0), new Position(2, 1)); // Qd2
            game.MakeMove(new Position(3, 7), new Position(2, 6)); // Qd7
            // Не двигаем короля и ладью - они должны остаться на начальных позициях для рокировки!

            // Act - длинная рокировка белых
            var result = game.MakeMove(new Position(4, 0), new Position(2, 0));

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(MoveType.Castle, result.MoveType);
            var king = game.Pieces.FirstOrDefault(p => p is King && p.Color == PieceColor.White) as King;
            var rook = game.Pieces.FirstOrDefault(p => p is Rook && p.Position == new Position(3, 0) && p.Color == PieceColor.White) as Rook;
            Assert.NotNull(king);
            Assert.NotNull(rook);
            Assert.Equal(new Position(2, 0), king.Position);
            Assert.Equal(new Position(3, 0), rook.Position);
        }

        [Fact]
        public void CannotCastle_IfKingHasMoved()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(4, 6), new Position(4, 4)); // e5
            game.MakeMove(new Position(4, 0), new Position(4, 1)); // Ke2 - король ходил
            game.MakeMove(new Position(4, 7), new Position(4, 6)); // Ke7
            game.MakeMove(new Position(4, 1), new Position(4, 0)); // Ke1 - вернулся

            // Act - пытаемся сделать рокировку после того, как король ходил
            var result = game.MakeMove(new Position(4, 0), new Position(6, 0));

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void CannotCastle_IfRookHasMoved()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(0, 1), new Position(0, 3)); // a4
            game.MakeMove(new Position(0, 6), new Position(0, 4)); // a5
            game.MakeMove(new Position(0, 0), new Position(0, 1)); // Ra2 - ладья ходила
            game.MakeMove(new Position(0, 7), new Position(0, 6)); // Ra7
            game.MakeMove(new Position(0, 1), new Position(0, 0)); // Ra1 - вернулась

            // Act - пытаемся сделать длинную рокировку после того, как ладья ходила
            var result = game.MakeMove(new Position(4, 0), new Position(2, 0));

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void CannotCastle_ThroughAttackedSquares()
        {
            // Arrange
            var game = new Game();
            // Освобождаем путь для рокировки
            game.MakeMove(new Position(5, 1), new Position(5, 3)); // f3
            game.MakeMove(new Position(6, 1), new Position(6, 3)); // g3
            game.MakeMove(new Position(3, 7), new Position(5, 5)); // Qf6 - атакует f1
            game.MakeMove(new Position(4, 7), new Position(5, 7)); // Kf8

            // Act - пытаемся сделать короткую рокировку через атакованную клетку
            var result = game.MakeMove(new Position(4, 0), new Position(6, 0));

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void CannotCastle_IfKingIsInCheck()
        {
            // Arrange
            var game = new Game();
            // Освобождаем путь для рокировки
            game.MakeMove(new Position(5, 1), new Position(5, 3)); // f3
            game.MakeMove(new Position(6, 1), new Position(6, 3)); // g3
            game.MakeMove(new Position(3, 7), new Position(3, 3)); // Qd4 - шах
            game.MakeMove(new Position(4, 7), new Position(5, 7)); // Kf8

            // Act - пытаемся сделать рокировку, когда король под шахом
            var result = game.MakeMove(new Position(4, 0), new Position(6, 0));

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void CannotCastle_IfPiecesBetweenKingAndRook()
        {
            // Arrange
            var game = new Game();
            // Не освобождаем путь - между королем и ладьей есть фигуры

            // Act - пытаемся сделать рокировку
            var result = game.MakeMove(new Position(4, 0), new Position(6, 0));

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Black_CanPerformShortCastling()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(5, 1), new Position(5, 3)); // f3
            game.MakeMove(new Position(5, 6), new Position(5, 4)); // f5
            game.MakeMove(new Position(6, 1), new Position(6, 3)); // g3
            game.MakeMove(new Position(6, 6), new Position(6, 4)); // g5
            // Не двигаем короля и ладью черных!

            // Act - короткая рокировка черных
            var result = game.MakeMove(new Position(4, 7), new Position(6, 7));

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(MoveType.Castle, result.MoveType);
        }
    }
}


