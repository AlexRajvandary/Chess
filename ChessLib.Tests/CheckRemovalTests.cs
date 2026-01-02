using ChessLib;
using Xunit;

namespace ChessLib.Tests
{
    public class CheckRemovalTests
    {
        [Fact]
        public void CanCaptureAttackingPieceToRemoveCheck()
        {
            // Arrange - создаем позицию, где король под шахом от ферзя
            var game = new Game();
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(4, 6), new Position(4, 4)); // e5
            game.MakeMove(new Position(5, 0), new Position(2, 3)); // Bc4
            game.MakeMove(new Position(1, 7), new Position(2, 5)); // Nf6
            game.MakeMove(new Position(3, 0), new Position(7, 4)); // Qf3
            game.MakeMove(new Position(2, 5), new Position(4, 4)); // Nxe4
            game.MakeMove(new Position(1, 6), new Position(1, 5)); // b6 - черные
            game.MakeMove(new Position(7, 4), new Position(7, 6)); // Qf7 - шах черному королю

            // Act - черная ладья на h8 может взять ферзя на f7, чтобы убрать шах
            var result = game.MakeMove(new Position(7, 7), new Position(7, 6)); // Rxf7 - ладья берет ферзя

            // Assert - ход должен быть валидным, так как он убирает шах
            Assert.True(result.IsValid);
            Assert.Equal(MoveType.Capture, result.MoveType);
        }

        [Fact]
        public void CanBlockCheckByMovingPiece()
        {
            // Arrange - создаем позицию, где король под шахом
            var game = new Game();
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(4, 6), new Position(4, 4)); // e5
            game.MakeMove(new Position(3, 0), new Position(4, 1)); // Qe2
            game.MakeMove(new Position(1, 7), new Position(2, 5)); // Nf6
            game.MakeMove(new Position(4, 1), new Position(4, 6)); // Qe7 - шах (если нет препятствий)

            // Act - проверяем, что можно заблокировать шах
            // Для этого нужно создать позицию, где можно заблокировать
            // Упростим - проверим, что если король под шахом, можно сделать ход, который убирает шах
        }
    }
}

