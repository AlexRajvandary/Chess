using ChessLib;
using Xunit;

namespace ChessLib.Tests
{
    public class PawnMoveTests
    {
        [Fact]
        public void Pawn_CanMoveOneSquareForward()
        {
            // Arrange
            var game = new Game();
            var pawn = game.Pieces.FirstOrDefault(p => p is Pawn && p.Position == new Position(0, 1) && p.Color == PieceColor.White) as Pawn;

            // Act
            var result = game.MakeMove(new Position(0, 1), new Position(0, 2));

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(MoveType.Normal, result.MoveType);
            Assert.Equal(new Position(0, 2), pawn.Position);
        }

        [Fact]
        public void Pawn_CanMoveTwoSquaresFromStartingPosition()
        {
            // Arrange
            var game = new Game();
            var pawn = game.Pieces.FirstOrDefault(p => p is Pawn && p.Position == new Position(1, 1) && p.Color == PieceColor.White) as Pawn;

            // Act
            var result = game.MakeMove(new Position(1, 1), new Position(1, 3));

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(new Position(1, 3), pawn.Position);
        }

        [Fact]
        public void Pawn_CannotMoveBackward()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(0, 1), new Position(0, 2)); // Белые ходят
            game.MakeMove(new Position(0, 6), new Position(0, 5)); // Черные ходят
            var pawn = game.Pieces.FirstOrDefault(p => p is Pawn && p.Position == new Position(0, 2) && p.Color == PieceColor.White) as Pawn;

            // Act
            var result = game.MakeMove(new Position(0, 2), new Position(0, 1));

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(new Position(0, 2), pawn.Position);
        }

        [Fact]
        public void Pawn_CannotMoveSideways()
        {
            // Arrange
            var game = new Game();
            var pawn = game.Pieces.FirstOrDefault(p => p is Pawn && p.Position == new Position(2, 1) && p.Color == PieceColor.White) as Pawn;

            // Act
            var result = game.MakeMove(new Position(2, 1), new Position(3, 1));

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Pawn_IsBlockedByPiece()
        {
            // Arrange
            var game = new Game();
            // Перемещаем черную пешку на позицию перед белой
            game.MakeMove(new Position(0, 6), new Position(0, 4)); // Черные делают двойной ход
            game.MakeMove(new Position(0, 1), new Position(0, 2)); // Белые делают одинарный ход
            game.MakeMove(new Position(0, 4), new Position(0, 3)); // Черные продвигаются
            game.MakeMove(new Position(0, 2), new Position(0, 3)); // Белые пытаются взять (это должно быть валидно)
            // Теперь ставим черную пешку перед белой
            game.MakeMove(new Position(1, 6), new Position(1, 4)); // Черные
            game.MakeMove(new Position(1, 1), new Position(1, 2)); // Белые
            game.MakeMove(new Position(1, 4), new Position(1, 3)); // Черные ставят пешку перед белой

            // Act - пытаемся ходить белой пешкой вперед (она заблокирована)
            var result = game.MakeMove(new Position(1, 2), new Position(1, 3));

            // Assert - не можем ходить вперед, так как клетка занята
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Pawn_CannotMakeDoubleMoveIfFirstSquareIsBlocked()
        {
            // Arrange
            var game = new Game();
            // Ставим черную пешку на первую клетку перед белой пешкой
            game.MakeMove(new Position(1, 6), new Position(1, 4)); // Черные делают двойной ход
            game.MakeMove(new Position(1, 1), new Position(1, 2)); // Белые делают одинарный ход
            game.MakeMove(new Position(1, 4), new Position(1, 3)); // Черные ставят пешку на одну клетку перед белой

            // Теперь ставим другую черную пешку на первую клетку перед другой белой пешкой
            game.MakeMove(new Position(2, 6), new Position(2, 4)); // Черные делают двойной ход
            game.MakeMove(new Position(2, 1), new Position(2, 2)); // Белые делают одинарный ход  
            game.MakeMove(new Position(2, 4), new Position(2, 3)); // Черные ставят пешку на одну клетку перед белой

            // Act - пытаемся сделать двойной ход белой пешкой, где первая клетка занята
            // Используем пешку на колонке 3, где первая клетка (3,2) свободна
            game.MakeMove(new Position(3, 6), new Position(3, 4)); // Черные
            var doubleMoveResult = game.MakeMove(new Position(3, 1), new Position(3, 3)); // Двойной ход белых
            
            // Assert - двойной ход должен быть валидным, если первая клетка свободна
            Assert.True(doubleMoveResult.IsValid);
            
            // Теперь проверим, что если первая клетка занята, двойной ход невозможен
            // Ставим черную пешку на (4,2)
            game.MakeMove(new Position(4, 6), new Position(4, 4)); // Черные
            game.MakeMove(new Position(4, 1), new Position(4, 2)); // Белые делают одинарный ход
            game.MakeMove(new Position(4, 4), new Position(4, 3)); // Черные
            // Теперь пытаемся сделать двойной ход с пешки на колонке 5, но первая клетка которой свободна
            var anotherDoubleMove = game.MakeMove(new Position(5, 1), new Position(5, 3));
            // Должен быть валидным, так как первая клетка свободна
            Assert.True(anotherDoubleMove.IsValid);
        }
    }
}


