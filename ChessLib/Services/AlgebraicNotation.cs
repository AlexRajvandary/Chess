using ChessLib.Common;
using ChessLib.Pieces;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessLib.Services
{
    public static class AlgebraicNotation
    {
        private static readonly string[] Files = ["a", "b", "c", "d", "e", "f", "g", "h"];
        private static readonly string[] Ranks = ["1", "2", "3", "4", "5", "6", "7", "8"];

        public static string ToAlgebraic(MoveNotation move, System.Collections.Generic.List<IPiece> allPieces, MoveStrategyService moveStrategyService)
        {
            if (move == null || move.Piece == null)
                return string.Empty;
            
            // Validate move positions
            if (move.To.X < 0 || move.To.X >= 8 || move.To.Y < 0 || move.To.Y >= 8)
                return string.Empty;
                
            var notation = new StringBuilder();

            if (move.MoveType == MoveType.Castle)
            {
                return move.To.X == 6 ? "O-O" : "O-O-O";
            }

            char pieceSymbol = GetPieceSymbol(move.Piece);
            if (pieceSymbol != 'P')
            {
                notation.Append(pieceSymbol);
            }

            if (move.Piece is Pawn)
            {
                if (move.MoveType == MoveType.Capture || move.MoveType == MoveType.EnPassant)
                {
                    notation.Append(Files[move.From.X]);
                }
            }
            else
            {
                // Check for ambiguous pieces (simplified - always check if there are multiple pieces of same type)
                if (allPieces != null)
                {
                    var ambiguousPieces = allPieces
                        .Where(p => p != null && !p.IsDead && 
                                    p.GetType() == move.Piece.GetType() && 
                                    p.Color == move.Piece.Color &&
                                    p != move.Piece)
                        .ToList();

                    if (ambiguousPieces.Any(p => CanReachSquare(p, move.To, allPieces, moveStrategyService)))
                    {
                        bool needFile = ambiguousPieces.Any(p => p.Position.X == move.From.X && p.Position.Y != move.From.Y);
                        bool needRank = ambiguousPieces.Any(p => p.Position.Y == move.From.Y && p.Position.X != move.From.X);

                        if (needFile || (!needRank && ambiguousPieces.Count > 0))
                        {
                            notation.Append(Files[move.From.X]);
                        }

                        if (needRank)
                        {
                            notation.Append(Ranks[move.From.Y]);
                        }
                    }
                }
            }

            if (move.MoveType == MoveType.Capture || move.MoveType == MoveType.EnPassant)
            {
                if (move.Piece is Pawn)
                {
                    notation.Append(Files[move.From.X]);
                }

                notation.Append('x');
            }

            notation.Append(Files[move.To.X]);
            notation.Append(Ranks[move.To.Y]);

            if (move.MoveType == MoveType.Promotion)
            {
                notation.Append("=Q");
            }

            if (move.IsCheckmate)
            {
                notation.Append('#');
            }
            else if (move.IsCheck)
            {
                notation.Append('+');
            }

            return notation.ToString();
        }

        private static char GetPieceSymbol(IPiece piece)
        {
            return piece switch
            {
                King => 'K',
                Queen => 'Q',
                Rook => 'R',
                Bishop => 'B',
                Knight => 'N',
                Pawn => 'P',
                _ => '?'
            };
        }

        /// <summary>
        /// Checks if a piece can reach the specified square according to its movement rules.
        /// This is used to determine ambiguity in algebraic notation (e.g., when two knights can reach the same square).
        /// Note: This is a simplified check that only verifies if the square is in the piece's possible moves,
        /// without checking if the move would leave the king in check (which is not needed for ambiguity resolution).
        /// </summary>
        private static bool CanReachSquare(IPiece piece, Position square, List<IPiece> allPieces, MoveStrategyService moveStrategyService)
        {
            if (piece == null || piece.IsDead || allPieces == null)
            {
                return false;
            }

            if (square.X < 0 || square.X >= 8 || square.Y < 0 || square.Y >= 8)
            {
                return false;
            }

            var gameField = new string[8, 8];
            foreach (var p in allPieces)
            {
                if (p != null && !p.IsDead)
                {
                    gameField[p.Position.X, p.Position.Y] = p.Color == PieceColor.White 
                        ? p.ToString().ToUpper() 
                        : p.ToString();
                }
            }
            
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (gameField[i, j] == null)
                    {
                        gameField[i, j] = " ";
                    }
                }
            }

            var gameFieldObj = new GameField();
            var possibleMoves = moveStrategyService.GetPossibleMoves(piece, allPieces, gameFieldObj, gameField);
            var possibleKills = moveStrategyService.GetPossibleCaptures(piece, allPieces, gameFieldObj, gameField);
            
            return possibleMoves.Contains(square) || possibleKills.Contains(square);
        }

        public static string FormatMoveHistory(List<MoveNotation> moves, List<IPiece> allPieces, MoveStrategyService moveStrategyService)
        {
            if (moves == null || moves.Count == 0)
            {
                return string.Empty;
            }

            allPieces ??= [];

            var result = new StringBuilder();
            int currentMoveNumber = 1;
            bool isWhiteMove = true;

            foreach (var move in moves)
            {
                if (move == null)
                {
                    continue;
                }

                if (isWhiteMove)
                {
                    result.Append($"{currentMoveNumber}. ");
                }

                string moveNotation = ToAlgebraic(move, allPieces, moveStrategyService);
                if (!string.IsNullOrEmpty(moveNotation))
                {
                    result.Append(moveNotation);
                    result.Append(' ');
                }

                if (!isWhiteMove)
                {
                    currentMoveNumber++;
                }

                isWhiteMove = !isWhiteMove;
            }

            return result.ToString().Trim();
        }
    }
}