using System;
using System.Collections.ObjectModel;
using System.Linq;
using ChessLib.Pieces;
using ChessWPF.Models;
using ChessWPF.Services;

namespace ChessWPF.ViewModels
{
    public class CapturedPiecesViewModel : NotifyPropertyChanged
    {
        private readonly ChessGameService gameService;
        private ObservableCollection<CapturedPieceInfo> capturedByWhite;
        private ObservableCollection<CapturedPieceInfo> capturedByBlack;

        public CapturedPiecesViewModel(ChessGameService gameService)
        {
            this.gameService = gameService
                ?? throw new ArgumentNullException(nameof(gameService));
            
            CapturedByWhite = [];
            CapturedByBlack = [];
        }

        public ObservableCollection<CapturedPieceInfo> CapturedByWhite
        {
            get => capturedByWhite;
            set
            {
                capturedByWhite = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CapturedPieceInfo> CapturedByBlack
        {
            get => capturedByBlack;
            set
            {
                capturedByBlack = value;
                OnPropertyChanged();
            }
        }

        public void AddCapturedPiece(PieceColor capturingPlayerColor, CellUIState capturedState)
        {
            var targetCollection = capturingPlayerColor == PieceColor.White ? CapturedByWhite : CapturedByBlack;
            var existingPiece = targetCollection.FirstOrDefault(p => p.Piece == capturedState);
            
            if (existingPiece != null)
            {
                existingPiece.Count++;
            }
            else
            {
                targetCollection.Add(new CapturedPieceInfo { Piece = capturedState, Count = 1 });
            }
        }

        public void UpdateFromMoveHistory(Func<IPiece, CellUIState> getStateFromPiece)
        {
            CapturedByWhite.Clear();
            CapturedByBlack.Clear();

            if (gameService?.CurrentGame?.MoveHistory == null)
            {
                System.Diagnostics.Debug.WriteLine("UpdateCapturedPieces: MoveHistory is null");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"UpdateCapturedPieces: Processing {gameService.CurrentGame.MoveHistory.Count} moves");

            foreach (var move in gameService.CurrentGame.MoveHistory)
            {
                if (move.CapturedPiece != null)
                {
                    var capturedState = getStateFromPiece(move.CapturedPiece);
                    var targetCollection = move.PlayerColor == PieceColor.White ? CapturedByWhite : CapturedByBlack;
                    var existingPiece = targetCollection.FirstOrDefault(p => p.Piece == capturedState);
                    
                    if (existingPiece != null)
                    {
                        existingPiece.Count++;
                    }
                    else
                    {
                        targetCollection.Add(new CapturedPieceInfo { Piece = capturedState, Count = 1 });
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"UpdateCapturedPieces: White captured {CapturedByWhite.Count}, Black captured {CapturedByBlack.Count}");
        }

        public void Clear()
        {
            CapturedByWhite.Clear();
            CapturedByBlack.Clear();
        }
    }
}