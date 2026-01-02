using ChessLib.Pieces;

namespace ChessLib.Common
{
    public class MoveResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public MoveType MoveType { get; set; }
        public IPiece CapturedPiece { get; set; }
        public bool IsCheck { get; set; }
        public bool IsCheckmate { get; set; }

        public static MoveResult Success(MoveType moveType = MoveType.Normal)
        {
            return new MoveResult
            {
                IsValid = true,
                MoveType = moveType
            };
        }

        public static MoveResult Failure(string errorMessage)
        {
            return new MoveResult
            {
                IsValid = false,
                ErrorMessage = errorMessage
            };
        }
    }
}