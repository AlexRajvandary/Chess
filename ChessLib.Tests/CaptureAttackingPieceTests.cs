using ChessLib;
using Xunit;

namespace ChessLib.Tests
{
    /// <summary>
    /// Тесты для проверки, что можно взять фигуру, которая атакует короля
    /// </summary>
    public class CaptureAttackingPieceTests
    {
        [Fact]
        public void CanCapturePieceThatIsAttackingKing()
        {
            // Arrange - создаем позицию, где ферзь атакует короля
            var game = new Game();
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(4, 6), new Position(4, 4)); // e5
            game.MakeMove(new Position(5, 0), new Position(2, 3)); // Bc4
            game.MakeMove(new Position(3, 7), new Position(7, 3)); // Qf5 - черный ферзь атакует белого короля
            // Теперь белый король под шахом от черного ферзя на f5

            // Проверяем, что король действительно под шахом
            Assert.True(game.IsCheck(PieceColor.White), "Король должен быть под шахом перед взятием фигуры");

            // Act - белая ладья берет черного ферзя, который атакует короля
            var result = game.MakeMove(new Position(7, 0), new Position(7, 3));

            // Assert - ход должен быть валидным, так как мы убираем шах, взяв атакующую фигуру
            Assert.True(result.IsValid, $"Ход должен быть валидным, но получили ошибку: {result.ErrorMessage}");
            Assert.Equal(MoveType.Capture, result.MoveType);
            Assert.NotNull(result.CapturedPiece);
            
            // Проверяем, что шах убран
            Assert.False(game.IsCheck(PieceColor.White));
        }

        [Fact]
        public void CanCapturePieceThatIsAttackingKing_WithPawn()
        {
            // Arrange - создаем позицию, где пешка атакует короля
            var game = new Game();
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(3, 6), new Position(3, 4)); // d5
            game.MakeMove(new Position(4, 0), new Position(4, 1)); // Ke2
            game.MakeMove(new Position(3, 4), new Position(4, 3)); // dxe4 - черная пешка берет и атакует короля
            // Теперь белый король под шахом от черной пешки на e4

            // Act - белая пешка берет черную пешку, которая атакует короля
            var result = game.MakeMove(new Position(5, 1), new Position(4, 3));

            // Assert - ход должен быть валидным
            Assert.True(result.IsValid);
            Assert.Equal(MoveType.Capture, result.MoveType);
            
            // Проверяем, что шах убран
            Assert.False(game.IsCheck(PieceColor.White));
        }

        [Fact]
        public void CanCapturePieceThatIsAttackingKing_WithBishop()
        {
            // Arrange - создаем позицию, где слон атакует короля
            var game = new Game();
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(4, 6), new Position(4, 4)); // e5
            game.MakeMove(new Position(4, 0), new Position(4, 1)); // Ke2
            game.MakeMove(new Position(5, 7), new Position(1, 3)); // Bc4 - черный слон атакует белого короля
            // Теперь белый король под шахом от черного слона на c4

            // Act - белая пешка берет черного слона, который атакует короля
            var result = game.MakeMove(new Position(1, 1), new Position(1, 3));

            // Assert - ход должен быть валидным
            Assert.True(result.IsValid);
            Assert.Equal(MoveType.Capture, result.MoveType);
            
            // Проверяем, что шах убран
            Assert.False(game.IsCheck(PieceColor.White));
        }
    }
}

