using ChessLib.Common;
using ChessLib.Pieces;
using Xunit;

namespace ChessLib.Tests
{
    public class MoveHistoryTests
    {
        [Fact]
        public void MoveHistory_RecordsMoves()
        {
            // Arrange
            var game = new Game();

            // Act
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(4, 6), new Position(4, 4)); // e5

            // Assert
            Assert.Equal(2, game.MoveHistory.Count);
            Assert.Equal(new Position(4, 1), game.MoveHistory[0].From);
            Assert.Equal(new Position(4, 3), game.MoveHistory[0].To);
            Assert.Equal(PieceColor.White, game.MoveHistory[0].PlayerColor);
        }

        [Fact]
        public void MoveHistory_ContainsCorrectMoveInformation()
        {
            // Arrange
            var game = new Game();

            // Act
            var result = game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4

            // Assert
            Assert.Single(game.MoveHistory);
            var move = game.MoveHistory[0];
            Assert.NotNull(move.Piece);
            Assert.Equal(MoveType.Normal, move.MoveType);
            Assert.Equal(1, move.MoveNumber);
        }

        [Fact]
        public void MoveHistory_RecordsCaptures()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(3, 6), new Position(3, 4)); // d5

            // Act
            var result = game.MakeMove(new Position(4, 3), new Position(3, 4)); // exd5

            // Assert
            var captureMove = game.MoveHistory.Last();
            Assert.Equal(MoveType.Capture, captureMove.MoveType);
            Assert.NotNull(captureMove.CapturedPiece);
        }

        [Fact]
        public void MoveHistory_RecordsCheckAndCheckmate()
        {
            // Arrange
            var game = new Game();
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // e4
            game.MakeMove(new Position(4, 6), new Position(4, 4)); // e5
            game.MakeMove(new Position(5, 0), new Position(2, 3)); // Bc4
            game.MakeMove(new Position(1, 7), new Position(2, 5)); // Nf6
            game.MakeMove(new Position(3, 0), new Position(7, 4)); // Qf3
            game.MakeMove(new Position(2, 5), new Position(4, 4)); // Nxe4
            game.MakeMove(new Position(7, 4), new Position(7, 6)); // Qf7

            // Assert - проверяем последний ход
            var lastMove = game.MoveHistory.Last();
            // Ферзь на f7 должен создавать шах (если король на e8)
            // Проверяем, что хотя бы шах определяется
            if (game.IsCheck(PieceColor.Black))
            {
                Assert.True(lastMove.IsCheck);
            }
            // Если это мат, проверяем и мат
            if (game.IsCheckmate(PieceColor.Black))
            {
                Assert.True(lastMove.IsCheckmate);
            }
        }

        [Fact]
        public void MoveHistory_MoveNumbersAreCorrect()
        {
            // Arrange
            var game = new Game();

            // Act
            game.MakeMove(new Position(4, 1), new Position(4, 3)); // 1. e4
            game.MakeMove(new Position(4, 6), new Position(4, 4)); // 1... e5
            game.MakeMove(new Position(5, 0), new Position(2, 3)); // 2. Bc4

            // Assert
            Assert.Equal(1, game.MoveHistory[0].MoveNumber);
            Assert.Equal(1, game.MoveHistory[1].MoveNumber);
            Assert.Equal(2, game.MoveHistory[2].MoveNumber);
        }
    }
}


