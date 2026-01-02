using System;
using System.Windows;
using System.Windows.Threading;
using ChessLib.Pieces;
using ChessWPF.Models;
using ChessWPF.Services;

namespace ChessWPF.ViewModels
{
    public class TimerViewModel : NotifyPropertyChanged
    {
        private readonly IGameService gameService;
        private readonly DispatcherTimer gameTimer;
        private readonly SoundService soundService;
        private TimeSpan blackPlayerTime = TimeSpan.Zero;
        private bool currentGameHasTimers = false;
        private TimeSpan initialTimePerPlayer = TimeSpan.FromMinutes(10);
        private bool isFirstMove = true;
        private bool isTimerEnabled = false;
        private bool isWhitePlayerActive = true;
        private TimeOption selectedTimeOption;
        private TimeSpan whitePlayerTime = TimeSpan.Zero;
        
        public TimerViewModel(IGameService gameService, SoundService soundService)
        {
            this.gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            this.soundService = soundService ?? throw new ArgumentNullException(nameof(soundService));
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1);
            gameTimer.Tick += GameTimer_Tick;
            SelectedTimeOption = new TimeOption { Time = TimeSpan.FromMinutes(10), Display = "10 minutes" };
        }

        public TimeSpan BlackPlayerTime
        {
            get => blackPlayerTime;
            set
            {
                blackPlayerTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BlackPlayerTimeFormatted));
            }
        }

        public string BlackPlayerTimeFormatted => FormatTime(blackPlayerTime);
        
        public TimeSpan InitialTimePerPlayer
        {
            get => initialTimePerPlayer;
            set
            {
                initialTimePerPlayer = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsTimeExpired => currentGameHasTimers && !gameTimer.IsEnabled && 
                                     (whitePlayerTime.TotalSeconds <= 0 || blackPlayerTime.TotalSeconds <= 0);
        
        public bool IsTimerEnabled
        {
            get => isTimerEnabled;
            set
            {
                isTimerEnabled = value;
                OnPropertyChanged();
            }
        }

        public TimeOption SelectedTimeOption
        {
            get => selectedTimeOption;
            set
            {
                selectedTimeOption = value;
                if (value != null)
                {
                    InitialTimePerPlayer = value.Time;
                }
                OnPropertyChanged();
            }
        }

        public bool ShowTimers => currentGameHasTimers;

        public TimeSpan WhitePlayerTime
        {
            get => whitePlayerTime;
            set
            {
                whitePlayerTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WhitePlayerTimeFormatted));
            }
        }

        public string WhitePlayerTimeFormatted => FormatTime(whitePlayerTime);
        
        public void InitializeForNewGame()
        {
            currentGameHasTimers = isTimerEnabled && selectedTimeOption != null;
            OnPropertyChanged(nameof(ShowTimers));
            if (currentGameHasTimers)
            {
                WhitePlayerTime = selectedTimeOption.Time;
                BlackPlayerTime = selectedTimeOption.Time;
                isWhitePlayerActive = true;
                isFirstMove = true;
                StopTimer();
            }
            else
            {
                StopTimer();
                isFirstMove = false;
            }
        }

        public void OnFirstPieceSelected(bool isWhitePiece, PieceColor currentPlayerColor)
        {
            if (currentGameHasTimers && isFirstMove && !gameTimer.IsEnabled)
            {
                if (isWhitePiece && currentPlayerColor == PieceColor.White)
                {
                    isWhitePlayerActive = true;
                    StartTimer();
                }
            }
        }

        public void OnGameEnd()
        {
            if (currentGameHasTimers)
            {
                StopTimer();
            }
        }

        public void ResetForLoadedGame()
        {
            currentGameHasTimers = false;
            OnPropertyChanged(nameof(ShowTimers));
            StopTimer();
        }

        public void StartTimer()
        {
            if (currentGameHasTimers)
            {
                gameTimer.Start();
            }
        }

        public void StopTimer()
        {
            gameTimer.Stop();
        }

        public void SwitchToNextPlayer()
        {
            if (currentGameHasTimers)
            {
                isFirstMove = false;
                isWhitePlayerActive = !isWhitePlayerActive;
            }
        }

        private void EndGameByTime(PieceColor losingColor)
        {
            StopTimer();
            gameService.EndGameByTime(losingColor);
            soundService.PlayCheckmateSound();
        }

        private string FormatTime(TimeSpan time)
        {
            if (time.TotalHours >= 1)
            {
                return $"{(int)time.TotalHours}:{time.Minutes:D2}:{time.Seconds:D2}";
            }
            return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (isWhitePlayerActive)
            {
                if (whitePlayerTime.TotalSeconds > 0)
                {
                    WhitePlayerTime = whitePlayerTime.Subtract(TimeSpan.FromSeconds(1));
                }
                else
                {
                    StopTimer();
                    EndGameByTime(PieceColor.White);
                    MessageBox.Show("White's time expired! Black wins on time.", "Time Expired", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (blackPlayerTime.TotalSeconds > 0)
                {
                    BlackPlayerTime = blackPlayerTime.Subtract(TimeSpan.FromSeconds(1));
                }
                else
                {
                    StopTimer();
                    EndGameByTime(PieceColor.Black);
                    MessageBox.Show("Black's time expired! White wins on time.", "Time Expired", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}