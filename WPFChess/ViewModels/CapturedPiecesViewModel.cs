using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ChessLib.Pieces;
using ChessWPF.Models;
using ChessWPF.Services;

namespace ChessWPF.ViewModels
{
    public class CapturedPiecesViewModel : NotifyPropertyChanged
    {
        private readonly IGameService gameService;
        private ObservableCollection<CapturedPieceInfo> capturedByWhite;
        private ObservableCollection<CapturedPieceInfo> capturedByBlack;

        public CapturedPiecesViewModel(IGameService gameService)
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

        /// <summary>
        /// Updates captured pieces from a list of moves with captured pieces
        /// </summary>
        public void UpdateFromCapturedPieces(List<(PieceColor capturingColor, IPiece capturedPiece)> capturedPieces, Func<IPiece, CellUIState> getStateFromPiece)
        {
            CapturedByWhite.Clear();
            CapturedByBlack.Clear();

            if (capturedPieces == null || getStateFromPiece == null)
            {
                return;
            }

            foreach (var (capturingColor, capturedPiece) in capturedPieces)
            {
                if (capturedPiece != null)
                {
                    var capturedState = getStateFromPiece(capturedPiece);
                    var targetCollection = capturingColor == PieceColor.White ? CapturedByWhite : CapturedByBlack;
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
        }

        public void Clear()
        {
            CapturedByWhite.Clear();
            CapturedByBlack.Clear();
        }
    }
}