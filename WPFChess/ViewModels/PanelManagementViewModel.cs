using System;
using System.Windows;
using System.Windows.Input;

namespace ChessWPF.ViewModels
{
    public class PanelManagementViewModel : NotifyPropertyChanged
    {
        private HorizontalAlignment aboutPanelAlignment = HorizontalAlignment.Left;
        private ICommand closeAboutCommand;
        private ICommand closeGameCommand;
        private ICommand closeSettingsCommand;
        private HorizontalAlignment gamePanelAlignment = HorizontalAlignment.Left;
        private bool isAboutPanelVisible;
        private bool isGamePanelVisible;
        private bool isSettingsPanelVisible;
        private bool isSidePanelVisible = true;
        private ICommand openAboutCommand;
        private ICommand openGameCommand;
        private ICommand openSettingsCommand;
        private HorizontalAlignment settingsPanelAlignment = HorizontalAlignment.Left;
        private ICommand toggleSidePanelCommand;
        
        public PanelManagementViewModel()
        {
        }
        
        public Action OnGamePanelOpened { get; set; }
        
        public HorizontalAlignment AboutPanelAlignment
        {
            get => aboutPanelAlignment;
            set
            {
                aboutPanelAlignment = value;
                OnPropertyChanged();
            }
        }
        
        public ICommand CloseAboutCommand => closeAboutCommand ??= new RelayCommand(parameter =>
        {
            IsAboutPanelVisible = false;
        });
        
        public ICommand CloseGameCommand => closeGameCommand ??= new RelayCommand(parameter =>
        {
            IsGamePanelVisible = false;
        });
        
        public ICommand CloseSettingsCommand => closeSettingsCommand ??= new RelayCommand(parameter =>
        {
            IsSettingsPanelVisible = false;
        });
        
        public HorizontalAlignment GamePanelAlignment
        {
            get => gamePanelAlignment;
            set
            {
                gamePanelAlignment = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsAboutPanelVisible
        {
            get => isAboutPanelVisible;
            set
            {
                isAboutPanelVisible = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsGamePanelVisible
        {
            get => isGamePanelVisible;
            set
            {
                isGamePanelVisible = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsSettingsPanelVisible
        {
            get => isSettingsPanelVisible;
            set
            {
                isSettingsPanelVisible = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsSidePanelVisible
        {
            get => isSidePanelVisible;
            set
            {
                isSidePanelVisible = value;
                OnPropertyChanged();
            }
        }
        
        public ICommand OpenAboutCommand => openAboutCommand ??= new RelayCommand(parameter =>
        {
            IsAboutPanelVisible = true;
        });
        
        public ICommand OpenGameCommand => openGameCommand ??= new RelayCommand(parameter =>
        {
            IsGamePanelVisible = true;
            OnGamePanelOpened?.Invoke();
        });
        
        public ICommand OpenSettingsCommand => openSettingsCommand ??= new RelayCommand(parameter =>
        {
            IsSettingsPanelVisible = true;
        });
        
        public HorizontalAlignment SettingsPanelAlignment
        {
            get => settingsPanelAlignment;
            set
            {
                settingsPanelAlignment = value;
                OnPropertyChanged();
            }
        }
        
        public ICommand ToggleSidePanelCommand => toggleSidePanelCommand ??= new RelayCommand(parameter =>
        {
            IsSidePanelVisible = !IsSidePanelVisible;
        });
        
        public void UpdateSettingsPanelAlignment(PanelPosition position)
        {
            SettingsPanelAlignment = position == PanelPosition.Left 
                ? HorizontalAlignment.Left 
                : HorizontalAlignment.Right;
        }
    }
}