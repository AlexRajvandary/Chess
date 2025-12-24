using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessLib
{
    public class Game
    {
        private readonly MoveValidator moveValidator;
        private readonly MoveExecutor moveExecutor;

        public GameField GameField { get; private set; }

        public int CurrentPlayer { get; private set; }

        public List<IPiece> Pieces { get; private set; }

        public List<Player> Players { get; private set; }

        public bool IsGameOver { get; private set; }

        public PieceColor? TimeLoser { get; private set; }

        public List<MoveNotation> MoveHistory { get; private set; }

        public PieceColor CurrentPlayerColor => CurrentPlayer % 2 == 0 ? PieceColor.White : PieceColor.Black;

        public static List<IPiece> GetPiecesStartPosition()
        {
            var Pieces = new List<IPiece>();
            //Create pawns
            for (int i = 0; i < 8; i++)
            {
                var wPawn = new Pawn(PieceColor.White, new Position(i, 1));
                var bPawn = new Pawn(PieceColor.Black, new Position(i, 6));
                Pieces.Add(wPawn);
                Pieces.Add(bPawn);
            }
            //create bishops
            Pieces.Add(new Bishop(new Position(2, 0), PieceColor.White));
            Pieces.Add(new Bishop(new Position(5, 0), PieceColor.White));
            Pieces.Add(new Bishop(new Position(2, 7), PieceColor.Black));
            Pieces.Add(new Bishop(new Position(5, 7), PieceColor.Black));

            //create rooks
            Pieces.Add(new Rook(new Position(0, 0), PieceColor.White));
            Pieces.Add(new Rook(new Position(7, 0), PieceColor.White));
            Pieces.Add(new Rook(new Position(0, 7), PieceColor.Black));
            Pieces.Add(new Rook(new Position(7, 7), PieceColor.Black));

            //create knights
            Pieces.Add(new Knight(new Position(1, 0), PieceColor.White));
            Pieces.Add(new Knight(new Position(6, 0), PieceColor.White));
            Pieces.Add(new Knight(new Position(1, 7), PieceColor.Black));
            Pieces.Add(new Knight(new Position(6, 7), PieceColor.Black));

            //create queens
            Pieces.Add(new Queen(PieceColor.Black, new Position(3, 7)));
            Pieces.Add(new Queen(PieceColor.White, new Position(3, 0)));

            //create kings
            Pieces.Add(new King(new Position(4, 0), PieceColor.White));
            Pieces.Add(new King(new Position(4, 7), PieceColor.Black));

            return Pieces;
        }

        public static string[,] GetGameField(List<IPiece> pieces)
        {
            string[,] GameField = new string[8, 8];
            foreach (var piece in pieces)
            {
                GameField[piece.Position.X, piece.Position.Y] = piece.Color == PieceColor.White ? piece.ToString().ToUpper() : piece.ToString();
            }
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (GameField[i, j] == null)
                    {
                        GameField[i, j] = " ";
                    }
                }
            }
            return GameField;
        }

        public Game()
        {
            CurrentPlayer = 0;
            Pieces = new List<IPiece>();
            Pieces = GetPiecesStartPosition();
            GameField = new GameField();
            moveValidator = new MoveValidator(GameField);
            moveExecutor = new MoveExecutor(GameField);
            MoveHistory = [];
            Player player1 = new(PieceColor.White, [.. Pieces.Where(x => x.Color == PieceColor.White)], "user1");
            Player player2 = new(PieceColor.Black, [.. Pieces.Where(x => x.Color == PieceColor.Black)], "user2");

            Players = new List<Player>
            {
                player1,
                player2
            };

            IsGameOver = false;
            TimeLoser = null;
        }

        public MoveResult MakeMove(Position from, Position to)
        {
            if (IsGameOver)
                return MoveResult.Failure("Game is over");

            var piece = Pieces.FirstOrDefault(p => p.Position == from && !p.IsDead);
            if (piece == null)
            {
                return MoveResult.Failure("No piece at source position");
            }

            if (piece.Color != CurrentPlayerColor)
            {
                return MoveResult.Failure("Not your turn");
            }

            var gameFieldString = GetGameField(Pieces);
            MoveResult result;

            if (piece is King king && !king.IsMoved)
            {
                var castlingResult = TryCastling(king, to, gameFieldString);
                if (castlingResult != null)
                {
                    result = castlingResult;
                }
                else
                {
                    var enPassantResult = TryEnPassant(piece, to, gameFieldString);
                    if (enPassantResult != null)
                    {
                        result = enPassantResult;
                    }
                    else
                    {
                        if (!moveValidator.IsValidMove(Pieces, piece, to, gameFieldString))
                        {
                            return MoveResult.Failure("Invalid move");
                        }

                        result = moveExecutor.ExecuteMove(Pieces, piece, to, gameFieldString);
                    }
                }
            }
            else
            {
                var enPassantResult = TryEnPassant(piece, to, gameFieldString);
                if (enPassantResult != null)
                {
                    result = enPassantResult;
                }
                else
                {
                    if (!moveValidator.IsValidMove(Pieces, piece, to, gameFieldString))
                    {
                        return MoveResult.Failure("Invalid move");
                    }

                    result = moveExecutor.ExecuteMove(Pieces, piece, to, gameFieldString);

                    if (piece is Pawn pawn)
                    {
                        CheckAndSetEnPassant(pawn, from, to);
                    }
                }
            }

            if (piece is King k)
            {
                k.IsMoved = true;
            }
            else if (piece is Rook r)
            {
                r.IsMoved = true;
            }

            moveExecutor.RemoveDeadPieces(Pieces);
            var moveNotation = new MoveNotation
            {
                From = from,
                To = to,
                Piece = piece,
                MoveType = result.MoveType,
                CapturedPiece = result.CapturedPiece,
                PlayerColor = CurrentPlayerColor,
                MoveNumber = (MoveHistory.Count / 2) + 1
            };

            UpdateGameField();
            SwitchPlayer();
            var nextPlayerColor = CurrentPlayerColor;
            result.IsCheck = IsCheck(nextPlayerColor);
            result.IsCheckmate = result.IsCheck && IsCheckmate(nextPlayerColor);
            moveNotation.IsCheck = result.IsCheck;
            moveNotation.IsCheckmate = result.IsCheckmate;
            MoveHistory.Add(moveNotation);

            if (result.IsCheckmate)
            {
                IsGameOver = true;
            }

            return result;
        }

        private MoveResult TryCastling(King king, Position to, string[,] gameFieldString)
        {
            int y = king.Position.Y;

            if (to.X == 6 && to.Y == y)
            {
                if (Pieces.FirstOrDefault(p => p is Rook r && r.Color == king.Color && !r.IsMoved && !r.IsDead && r.Position.X == 7 && r.Position.Y == y) is Rook rook && moveValidator.CanCastle(Pieces, king, rook, CastleType.Short, gameFieldString))
                {
                    return moveExecutor.ExecuteCastling(Pieces, king, rook, CastleType.Short);
                }
            }
            else if (to.X == 2 && to.Y == y)
            {
                if (Pieces.FirstOrDefault(p => p is Rook r && r.Color == king.Color && !r.IsMoved && !r.IsDead && r.Position.X == 0 && r.Position.Y == y) is Rook rook && moveValidator.CanCastle(Pieces, king, rook, CastleType.Long, gameFieldString))
                {
                    return moveExecutor.ExecuteCastling(Pieces, king, rook, CastleType.Long);
                }
            }

            return null;
        }

        private MoveResult TryEnPassant(IPiece piece, Position to, string[,] gameFieldString)
        {
            if (piece is not Pawn pawn)
            {
                return null;
            }

            if (to.X < 0 || to.X >= 8 || to.Y < 0 || to.Y >= 8)
            {
                return null;
            }

            if (gameFieldString[to.X, to.Y] != " ")
            {
                return null;
            }

            // Check if there's an enemy pawn that can be captured en passant
            var enemyPawns = Pieces.Where(p => p is Pawn p2 && p2.Color != pawn.Color && !p2.IsDead && p2.EnPassantAvailable).Cast<Pawn>().ToList();

            foreach (var enemyPawn in enemyPawns)
            {
                // Check if pawns are on the same rank (horizontal)
                if (pawn.Position.Y != enemyPawn.Position.Y)
                {
                    continue;
                }

                // Check if pawns are adjacent horizontally
                if (Math.Abs(pawn.Position.X - enemyPawn.Position.X) != 1)
                {
                    continue;
                }

                // Check if destination matches the en passant capture square
                // For white: captures black pawn on rank 5 (y=4), moves to rank 6 (y=5)
                // For black: captures white pawn on rank 4 (y=3), moves to rank 3 (y=2)
                int expectedY = pawn.Color == PieceColor.White ? enemyPawn.Position.Y + 1 : enemyPawn.Position.Y - 1;

                if (to.X == enemyPawn.Position.X && to.Y == expectedY)
                {
                    // Validate that this is a valid en passant move
                    if (pawn.AvailableEnPassent(enemyPawn))
                    {
                        return moveExecutor.ExecuteEnPassant(Pieces, pawn, to, enemyPawn);
                    }
                }
            }

            return null;
        }

        private void CheckAndSetEnPassant(Pawn pawn, Position from, Position to)
        {
            if (from == pawn.StartPos)
            {
                int moveDistance = System.Math.Abs(to.Y - from.Y);
                if (moveDistance == 2)
                {
                    pawn.EnPassantAvailable = true;
                }
                else
                {
                    pawn.EnPassantAvailable = false;
                }
            }
        }

        public List<Position> GetValidMoves(Position piecePosition)
        {
            var piece = Pieces.FirstOrDefault(p => p.Position == piecePosition && !p.IsDead);
            if (piece == null)
            {
                return [];
            }

            if (piece.Color != CurrentPlayerColor)
            {
                return [];
            }

            var gameFieldString = GetGameField(Pieces);
            var possibleMoves = piece.AvailableMoves(gameFieldString);
            var possibleKills = piece.AvailableKills(gameFieldString);

            if (piece is Pawn pawn)
            {
                /*
                 * Here we get destination positions to do enpassant:
                 * first, filter all pieces to get enemies alive pawns with available enpassant flag
                 * then, check if they are on the same position.Y and close by 1 in Position.X
                 * finally, we have to check if the destination cell is empty
                 * also, don't forget when we capture pawn with enpassant the destination cell isnt on the same position))
                 */
                var performEnPassantDestinations = Pieces
                    .Where(p =>
                    {
                        if (p is not Pawn ||
                            p.Color == pawn.Color ||
                            p.IsDead ||
                            (pawn.Color == PieceColor.White
                                ? pawn.Position.Y != 4 
                                : pawn.Position.Y != 3) ||
                            (p.Color == PieceColor.White
                                ? p.Position.Y != 3
                                : p.Position.Y != 4))
                        {
                            return false;
                        }

                        var enPassantAvailable = ((Pawn)p).EnPassantAvailable &&
                               pawn.Position.Y == p.Position.Y &&
                               Math.Abs(pawn.Position.X - p.Position.X) == 1;

                        var destination = pawn.Color is PieceColor.White
                            ? new Position(p.Position.X, p.Position.Y + 1)
                            : new Position(p.Position.X, p.Position.Y - 1);

                        var isCellFree = GameField.IsCellFree(destination);
                        return isCellFree && enPassantAvailable;
                    })
                    .Select(enemyPawn => pawn.Color is PieceColor.White
                        ? new Position(enemyPawn.Position.X, enemyPawn.Position.Y + 1)
                        : new Position(enemyPawn.Position.X, enemyPawn.Position.Y - 1));

                var possibleKillsWithEnPassant = possibleKills.Concat(performEnPassantDestinations);
                var allPossibleMoves = possibleMoves.Concat(possibleKillsWithEnPassant).ToList();
                return allPossibleMoves;
                //return moveValidator.FilterValidMoves(Pieces, piece, allPossibleMoves, gameFieldString);
            }
            else
            {
                var allPossibleMoves = possibleMoves.Concat(possibleKills).ToList();
                return moveValidator.FilterValidMoves(Pieces, piece, allPossibleMoves, gameFieldString);
            }
        }

        public List<Position> GetValidMovesForPiece(IPiece piece)
        {
            if (piece == null || piece.IsDead)
            {
                return [];
            }

            if (piece.Color != CurrentPlayerColor)
            {
                return [];
            }

            var gameFieldString = GetGameField(Pieces);
            var possibleMoves = piece.AvailableMoves(gameFieldString);
            var possibleKills = piece.AvailableKills(gameFieldString);
            var allPossibleMoves = possibleMoves.Concat(possibleKills).ToList();

            return moveValidator.FilterValidMoves(Pieces, piece, allPossibleMoves, gameFieldString);
        }

        public bool IsValidMove(Position from, Position to)
        {
            var piece = Pieces.FirstOrDefault(p => p.Position == from && !p.IsDead);
            if (piece == null)
            {
                return false;
            }

            if (piece.Color != CurrentPlayerColor)
            {
                return false;
            }

            var gameFieldString = GetGameField(Pieces);
            return moveValidator.IsValidMove(Pieces, piece, to, gameFieldString);
        }

        public bool IsCheck(PieceColor color)
        {
            UpdateGameField();
            var gameFieldString = GetGameField(Pieces);
            if (Pieces.FirstOrDefault(p => p is King && p.Color == color && !p.IsDead) is not King king)
            {
                return false;
            }

            var enemyPieces = Pieces.Where(p => p.Color != color && !p.IsDead).ToList();
            return GameField.GetAtackStatusStatic(enemyPieces, king.Position, gameFieldString);
        }

        public bool IsCheckmate(PieceColor color)
        {
            if (!IsCheck(color))
            {
                return false;
            }

            var playerPieces = Pieces.Where(p => p.Color == color && !p.IsDead).ToList();
            var gameFieldString = GetGameField(Pieces);

            foreach (var piece in playerPieces)
            {
                var possibleMoves = piece.AvailableMoves(gameFieldString);
                var possibleKills = piece.AvailableKills(gameFieldString);
                var allPossibleMoves = possibleMoves.Concat(possibleKills).ToList();

                foreach (var move in allPossibleMoves)
                {
                    if (moveValidator.IsValidMove(Pieces, piece, move, gameFieldString))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public GameState GetState()
        {
            var state = new GameState
            {
                CurrentPlayerColor = CurrentPlayerColor,
                Pieces = Pieces.ToList(), // Create a copy
                BoardRepresentation = GetGameField(Pieces),
                IsCheck = IsCheck(CurrentPlayerColor),
                IsCheckmate = IsCheckmate(CurrentPlayerColor),
                IsGameOver = IsGameOver
            };

            if (state.IsCheckmate)
            {
                state.IsGameOver = true;
                state.GameOverReason = $"{CurrentPlayerColor} is checkmated";
            }

            return state;
        }

        public void StartNewGame()
        {
            CurrentPlayer = 0;
            Pieces = GetPiecesStartPosition();
            GameField = new GameField();
            MoveHistory = new List<MoveNotation>();
            IsGameOver = false;
            TimeLoser = null;

            Players = new List<Player>
            {
                new Player(PieceColor.White, Pieces.Where(x => x.Color == PieceColor.White).ToList(), "user1"),
                new Player(PieceColor.Black, Pieces.Where(x => x.Color == PieceColor.Black).ToList(), "user2")
            };
        }

        public void EndGameByTime(PieceColor losingColor)
        {
            IsGameOver = true;
            TimeLoser = losingColor;
        }

        private void UpdateGameField()
        {
            var gameFieldString = GetGameField(Pieces);
            GameField.Update(Pieces, gameFieldString, CurrentPlayerColor);
        }

        private void SwitchPlayer()
        {
            CurrentPlayer++;
            if (CurrentPlayer >= 2)
            {
                CurrentPlayer = 0;
            }
        }

        public string GetFen()
        {
            return Fen.GenerateFen(this);
        }

        public string GetMoveHistory()
        {
            return AlgebraicNotation.FormatMoveHistory(MoveHistory, Pieces);
        }
    }
}