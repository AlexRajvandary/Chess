using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Threading;
using ChessLib.Pieces;
using ChessWPF.Models;
using ChessWPF.Services;

namespace ChessWPF.ViewModels
{
    public class MoveHistoryViewModel : NotifyPropertyChanged
    {
        private readonly DispatcherTimer autoPlayTimer;
        private readonly ChessGameService gameService;
        private bool isAutoPlaying = false;
        private bool isGameLoaded = false;
        private List<string> loadedGameMoves = new List<string>();
        private ObservableCollection<MoveDisplayItem> moveHistoryItems;
        private ICommand nextMoveCommand;
        private ICommand previousMoveCommand;
        private int selectedMoveIndex = -1;
        private ICommand toggleAutoPlayCommand;

        public MoveHistoryViewModel() 
        {
        }

        public MoveHistoryViewModel(ChessGameService gameService)
        {
            this.gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            MoveHistoryItems = new ObservableCollection<MoveDisplayItem>();
            autoPlayTimer = new DispatcherTimer();
            autoPlayTimer.Interval = TimeSpan.FromSeconds(1.0);
            autoPlayTimer.Tick += AutoPlayTimer_Tick;
        }

        public Action OnBoardUpdateRequired { get; set; }

        public Action OnCapturedPiecesUpdateRequired { get; set; }

        public Func<string> GetMoveHistoryString { get; set; }

        public Func<IPiece, CellUIState> GetStateFromPiece { get; set; }

        public string AutoPlayButtonText => isAutoPlaying ? "⏸ Pause" : "▶ Play";

        public bool IsAutoPlaying
        {
            get => isAutoPlaying;
            private set
            {
                isAutoPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AutoPlayButtonText));
                CommandManager.InvalidateRequerySuggested();
            }
        }
        
        public bool IsGameLoaded
        {
            get => isGameLoaded;
            set
            {
                isGameLoaded = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ObservableCollection<MoveDisplayItem> MoveHistoryItems
        {
            get => moveHistoryItems;
            set
            {
                moveHistoryItems = value;
                OnPropertyChanged();
            }
        }
        
        public ICommand NextMoveCommand => nextMoveCommand ??= new RelayCommand(
            _ =>
            {
                if (IsGameLoaded && SelectedMoveIndex < loadedGameMoves.Count - 1)
                {
                    NavigateToMove(SelectedMoveIndex + 1);
                }
            },
            _ => IsGameLoaded && SelectedMoveIndex < loadedGameMoves.Count - 1);
        
        public ICommand PreviousMoveCommand => previousMoveCommand ??= new RelayCommand(
            _ =>
            {
                if (IsGameLoaded && SelectedMoveIndex > -1)
                {
                    NavigateToMove(SelectedMoveIndex - 1);
                }
            },
            _ => IsGameLoaded && SelectedMoveIndex > -1);
        
        public int SelectedMoveIndex
        {
            get => selectedMoveIndex;
            set
            {
                selectedMoveIndex = value;
                OnPropertyChanged();
                UpdateMoveHistoryItems();
            }
        }
        
        public ICommand ToggleAutoPlayCommand => toggleAutoPlayCommand ??= new RelayCommand(
            _ =>
            {
                if (isAutoPlaying)
                {
                    StopAutoPlay();
                }
                else
                {
                    StartAutoPlay();
                }
            },
            _ => IsGameLoaded);
        
        public void ClearLoadedGame()
        {
            IsGameLoaded = false;
            loadedGameMoves.Clear();
            SelectedMoveIndex = -1;
            MoveHistoryItems.Clear();
            StopAutoPlay();
        }
        
        public void LoadGame(List<string> moves)
        {
            loadedGameMoves = moves ?? new List<string>();
            IsGameLoaded = true;
            NavigateToMove(loadedGameMoves.Count - 1);
        }
        
        public void NavigateToMove(int moveIndex)
        {
            if (!IsGameLoaded || moveIndex < -1 || moveIndex >= loadedGameMoves.Count)
            {
                return;
            }

            gameService.StartNewGame();
            for (int i = 0; i <= moveIndex; i++)
            {
                var moveNotation = loadedGameMoves[i];
                var moveInfo = PgnMoveParser.ParseMove(moveNotation, gameService.CurrentGame);
                if (moveInfo != null)
                {
                    _ = gameService.MakeMove(moveInfo.From, moveInfo.To);
                }
            }

            SelectedMoveIndex = moveIndex;
            OnBoardUpdateRequired?.Invoke();
            OnCapturedPiecesUpdateRequired?.Invoke();
            UpdateMoveHistoryItems();
            CommandManager.InvalidateRequerySuggested();
        }
        
        public void SelectMove(int moveIndex)
        {
            NavigateToMove(moveIndex);
        }
        
        public void UpdateMoveHistoryItems()
        {
            MoveHistoryItems ??= [];
            MoveHistoryItems.Clear();
            if (IsGameLoaded && loadedGameMoves != null && loadedGameMoves.Count > 0)
            {
                int moveNumber = 1;
                for (int i = 0; i < loadedGameMoves.Count; i += 2)
                {
                    int whiteIndex = i;
                    int blackIndex = i + 1;
                    string whiteMove = loadedGameMoves[whiteIndex];
                    string blackMove = blackIndex < loadedGameMoves.Count ? loadedGameMoves[blackIndex] : null;
                    var item = new MoveDisplayItem
                    {
                        MoveNumber = moveNumber,
                        WhiteMoveIndex = whiteIndex,
                        WhiteMoveText = whiteMove,
                        IsWhiteSelected = whiteIndex == SelectedMoveIndex,
                        SelectWhiteMoveCommand = new RelayCommand(_ => SelectMove(whiteIndex))
                    };
                    
                    if (blackMove != null)
                    {
                        item.BlackMoveIndex = blackIndex;
                        item.BlackMoveText = blackMove;
                        item.IsBlackSelected = blackIndex == SelectedMoveIndex;
                        item.SelectBlackMoveCommand = new RelayCommand(_ => SelectMove(blackIndex));
                    }
                    MoveHistoryItems.Add(item);
                    moveNumber++;
                }
                
                foreach (var item in MoveHistoryItems)
                {
                    item.IsWhiteSelected = item.WhiteMoveIndex == SelectedMoveIndex;
                    if (item.BlackMoveText != null)
                    {
                        item.IsBlackSelected = item.BlackMoveIndex == SelectedMoveIndex;
                    }
                }
            }
            else
            {
                if (GetMoveHistoryString != null)
                {
                    string moveHistory = GetMoveHistoryString.Invoke();
                    if (!string.IsNullOrEmpty(moveHistory))
                    {
                        ParseMoveHistoryString(moveHistory);
                    }
                    else
                    {
                        MoveHistoryItems.Clear();
                    }
                }
                else
                {
                    MoveHistoryItems.Clear();
                }
            }
            
            OnPropertyChanged(nameof(MoveHistoryItems));
        }

        private void AutoPlayTimer_Tick(object sender, EventArgs e)
        {
            if (IsGameLoaded && SelectedMoveIndex < loadedGameMoves.Count - 1)
            {
                NavigateToMove(SelectedMoveIndex + 1);
            }
            else
            {
                StopAutoPlay();
            }
        }
        
        private void ParseMoveHistoryString(string moveHistory)
        {
            if (string.IsNullOrWhiteSpace(moveHistory))
            {
                return;
            }
            
            moveHistory = Regex.Replace(moveHistory, @"\s+", " ").Trim();
            var regex = new Regex(@"(\d+)\.\s*(.+?)(?=\s*\d+\.|$)");
            var matches = regex.Matches(moveHistory);
            
            foreach (Match match in matches)
            {
                if (match.Success && match.Groups.Count >= 3)
                {
                    int moveNumber = int.Parse(match.Groups[1].Value);
                    string movesPair = match.Groups[2].Value.Trim();
                    var moves = movesPair.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    string whiteMove = moves.Length > 0 ? moves[0].TrimEnd('+', '#') : null;
                    string blackMove = moves.Length > 1 ? moves[1].TrimEnd('+', '#') : null;
                    if (!string.IsNullOrWhiteSpace(whiteMove))
                    {
                        var item = new MoveDisplayItem
                        {
                            MoveNumber = moveNumber,
                            WhiteMoveText = whiteMove,
                            BlackMoveText = blackMove
                        };
                        MoveHistoryItems.Add(item);
                    }
                }
            }
        }
        
        private void StartAutoPlay()
        {
            if (!IsGameLoaded)
                return;
            IsAutoPlaying = true;
            autoPlayTimer.Start();
        }
        
        private void StopAutoPlay()
        {
            IsAutoPlaying = false;
            autoPlayTimer.Stop();
        }
    }
}