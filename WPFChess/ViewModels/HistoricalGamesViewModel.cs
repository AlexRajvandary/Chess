using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ChessWPF.Models;
using ChessWPF.Services;

namespace ChessWPF.ViewModels
{
    public class HistoricalGamesViewModel : NotifyPropertyChanged
    {
        private int currentPage = 1;
        private long databaseSize = 0;
        private Brush importResultColor = Brushes.Black;
        private int importProgress = 0;
        private string importProgressText = "";
        private string importResultMessage = "";
        private ObservableCollection<HistoricalGame> historicalGames;
        private bool isImporting = false;
        private ICommand importPgnFileCommand;
        private ICommand loadSelectedHistoricalGameCommand;
        private ICommand nextPageCommand;
        private int pageSize = 7;
        private ICommand previousPageCommand;
        private ICommand refreshHistoricalGamesCommand;
        private HistoricalGame selectedHistoricalGame;
        private int totalCount = 0;
        private string whitePlayerFilter = "";
        private string blackPlayerFilter = "";
        private string eventFilter = "";
        private DateTime? dateFromFilter;
        private DateTime? dateToFilter;
        private string sortBy = "Date";
        private bool sortDescending = true;
        private ICommand applyFiltersCommand;
        private ICommand clearFiltersCommand;
        private ObservableCollection<int> availableYears;
        private ObservableCollection<object> availableYearsWithAll;
        private int? selectedYearFilter;
        
        private readonly GameStorageService _gameStorageService;

        public HistoricalGamesViewModel(GameStorageService gameStorageService)
        {
            _gameStorageService = gameStorageService ?? throw new ArgumentNullException(nameof(gameStorageService));
            HistoricalGames = new ObservableCollection<HistoricalGame>();
            availableYears = new ObservableCollection<int>();
            availableYearsWithAll = new ObservableCollection<object> { null };
            LoadAvailableYears();
        }
        
        public Action<HistoricalGame> OnGameLoadRequested { get; set; }
        
        public bool CanGoToNextPage
        {
            get
            {
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                return currentPage < totalPages;
            }
        }
        
        public bool CanGoToPreviousPage => currentPage > 1;
        
        public string CountInfo
        {
            get
            {
                if (totalCount == 0)
                    return "No games";
                return $"{totalCount} games ({DatabaseSizeFormatted})";
            }
        }
        
        public int CurrentPage
        {
            get => currentPage;
            set
            {
                currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageInfo));
                OnPropertyChanged(nameof(CanGoToPreviousPage));
                OnPropertyChanged(nameof(CanGoToNextPage));
            }
        }
        
        public long DatabaseSize
        {
            get => databaseSize;
            set
            {
                databaseSize = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DatabaseSizeFormatted));
                OnPropertyChanged(nameof(CountInfo));
            }
        }
        
        public string DatabaseSizeFormatted
        {
            get
            {
                if (databaseSize == 0)
                    return "0 B";
                double size = databaseSize;
                string[] units = { "B", "KB", "MB", "GB" };
                int unitIndex = 0;
                while (size >= 1024 && unitIndex < units.Length - 1)
                {
                    size /= 1024;
                    unitIndex++;
                }
                return $"{size:F2} {units[unitIndex]}";
            }
        }
        
        public ObservableCollection<HistoricalGame> HistoricalGames
        {
            get => historicalGames;
            set
            {
                historicalGames = value;
                OnPropertyChanged();
            }
        }
        
        public ICommand ImportPgnFileCommand => importPgnFileCommand ??= new RelayCommand(parameter =>
        {
            ImportPgnFile();
        }, parameter => !IsImporting);
        
        public int ImportProgress
        {
            get => importProgress;
            set
            {
                importProgress = value;
                OnPropertyChanged();
            }
        }
        
        public string ImportProgressText
        {
            get => importProgressText;
            set
            {
                importProgressText = value;
                OnPropertyChanged();
            }
        }
        
        public Brush ImportResultColor
        {
            get => importResultColor;
            set
            {
                importResultColor = value;
                OnPropertyChanged();
            }
        }
        
        public string ImportResultMessage
        {
            get => importResultMessage;
            set
            {
                importResultMessage = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsImporting
        {
            get => isImporting;
            set
            {
                isImporting = value;
                OnPropertyChanged();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
        
        public ICommand LoadSelectedHistoricalGameCommand => loadSelectedHistoricalGameCommand ??= new RelayCommand(parameter =>
        {
            if (SelectedHistoricalGame != null)
            {
                OnGameLoadRequested?.Invoke(SelectedHistoricalGame);
            }
        }, parameter => SelectedHistoricalGame != null);
        
        public ICommand NextPageCommand => nextPageCommand ??= new RelayCommand(parameter =>
        {
            if (CanGoToNextPage)
            {
                CurrentPage++;
                LoadHistoricalGames();
            }
        }, parameter => CanGoToNextPage);
        
        public string PageInfo
        {
            get
            {
                if (totalCount == 0)
                    return "No games";
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                int startIndex = (currentPage - 1) * pageSize + 1;
                int endIndex = Math.Min(currentPage * pageSize, totalCount);
                return $"Page {currentPage} of {totalPages} ({startIndex}-{endIndex} of {totalCount})";
            }
        }
        
        public int PageSize
        {
            get => pageSize;
            set
            {
                pageSize = value;
                OnPropertyChanged();
                LoadHistoricalGames();
            }
        }
        
        public ICommand PreviousPageCommand => previousPageCommand ??= new RelayCommand(parameter =>
        {
            if (CanGoToPreviousPage)
            {
                CurrentPage--;
                LoadHistoricalGames();
            }
        }, parameter => CanGoToPreviousPage);
        
        public ICommand RefreshHistoricalGamesCommand => refreshHistoricalGamesCommand ??= new RelayCommand(parameter =>
        {
            CurrentPage = 1;
            LoadHistoricalGames();
        });
        public string WhitePlayerFilter
        {
            get => whitePlayerFilter;
            set
            {
                whitePlayerFilter = value ?? "";
                OnPropertyChanged();
                CurrentPage = 1;
                LoadHistoricalGames();
            }
        }
        public string BlackPlayerFilter
        {
            get => blackPlayerFilter;
            set
            {
                blackPlayerFilter = value ?? "";
                OnPropertyChanged();
                CurrentPage = 1;
                LoadHistoricalGames();
            }
        }
        public string EventFilter
        {
            get => eventFilter;
            set
            {
                eventFilter = value ?? "";
                OnPropertyChanged();
                CurrentPage = 1;
                LoadHistoricalGames();
            }
        }
        public ObservableCollection<object> AvailableYearsWithAll
        {
            get => availableYearsWithAll;
            set
            {
                availableYearsWithAll = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<int> AvailableYears
        {
            get => availableYears;
            set
            {
                availableYears = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AvailableYearsWithAll));
            }
        }
        public object SelectedYearFilter
        {
            get => selectedYearFilter;
            set
            {
                if (value == null)
                {
                    selectedYearFilter = null;
                    dateFromFilter = null;
                    dateToFilter = null;
                }
                else if (value is int year)
                {
                    selectedYearFilter = year;
                    dateFromFilter = new DateTime(year, 1, 1);
                    dateToFilter = new DateTime(year, 12, 31);
                }
                OnPropertyChanged();
                CurrentPage = 1;
                LoadHistoricalGames();
            }
        }
        public string SortBy
        {
            get => sortBy;
            set
            {
                sortBy = value;
                OnPropertyChanged();
                CurrentPage = 1;
                LoadHistoricalGames();
            }
        }
        public bool SortDescending
        {
            get => sortDescending;
            set
            {
                sortDescending = value;
                OnPropertyChanged();
                CurrentPage = 1;
                LoadHistoricalGames();
            }
        }
        public ICommand ApplyFiltersCommand => applyFiltersCommand ??= new RelayCommand(parameter =>
        {
            CurrentPage = 1;
            LoadHistoricalGames();
        });
        public ICommand ClearFiltersCommand => clearFiltersCommand ??= new RelayCommand(parameter =>
        {
            WhitePlayerFilter = "";
            BlackPlayerFilter = "";
            EventFilter = "";
            SelectedYearFilter = null;
            SortBy = "Date";
            SortDescending = true;
            CurrentPage = 1;
            LoadHistoricalGames();
        });
        
        public HistoricalGame SelectedHistoricalGame
        {
            get => selectedHistoricalGame;
            set
            {
                selectedHistoricalGame = value;
                OnPropertyChanged();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
        
        public int TotalCount
        {
            get => totalCount;
            set
            {
                totalCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageInfo));
                OnPropertyChanged(nameof(CountInfo));
                OnPropertyChanged(nameof(CanGoToPreviousPage));
                OnPropertyChanged(nameof(CanGoToNextPage));
            }
        }
        
        public void LoadHistoricalGames()
        {
            try
            {
                var (games, totalCount) = GameStorageService.GetHistoricalGamesPaginated(
                    currentPage, 
                    pageSize,
                    string.IsNullOrWhiteSpace(whitePlayerFilter) ? null : whitePlayerFilter,
                    string.IsNullOrWhiteSpace(blackPlayerFilter) ? null : blackPlayerFilter,
                    string.IsNullOrWhiteSpace(eventFilter) ? null : eventFilter,
                    dateFromFilter,
                    dateToFilter,
                    sortBy,
                    sortDescending);
                TotalCount = totalCount;
                DatabaseSize = GameStorageService.GetDatabaseSize();
                HistoricalGames.Clear();
                foreach (var game in games)
                {
                    HistoricalGames.Add(game);
                }
                OnPropertyChanged(nameof(PageInfo));
                OnPropertyChanged(nameof(DatabaseSizeFormatted));
                OnPropertyChanged(nameof(CanGoToPreviousPage));
                OnPropertyChanged(nameof(CanGoToNextPage));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading historical games: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadAvailableYears()
        {
            try
            {
                var years = GameStorageService.GetAvailableYears();
                availableYears.Clear();
                availableYearsWithAll.Clear();
                availableYearsWithAll.Add(null);
                foreach (var year in years)
                {
                    availableYears.Add(year);
                    availableYearsWithAll.Add(year);
                }
                OnPropertyChanged(nameof(AvailableYears));
                OnPropertyChanged(nameof(AvailableYearsWithAll));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading years: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ImportPgnFile()
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "PGN files (*.pgn)|*.pgn|All files (*.*)|*.*",
                    Title = "Select PGN file to import"
                };
                var assetsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "HistoricalGames");
                if (System.IO.Directory.Exists(assetsPath))
                {
                    dialog.InitialDirectory = assetsPath;
                }
                if (dialog.ShowDialog() == true)
                {
                    var filePath = dialog.FileName;
                    var fileName = System.IO.Path.GetFileName(filePath);
                    if (GameStorageService.IsFileParsed(fileName))
                    {
                        ImportResultMessage = $"File {fileName} has already been imported.";
                        ImportResultColor = Brushes.Orange;
                        return;
                    }
                    IsImporting = true;
                    ImportProgress = 0;
                    ImportProgressText = "Starting import...";
                    ImportResultMessage = "";
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            var result = _gameStorageService.ImportPgnFile(filePath, (progress, current, total) =>
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    ImportProgress = progress;
                                    ImportProgressText = $"Imported: {current} of {total}";
                                });
                            });
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                IsImporting = false;
                                ImportProgress = 100;
                                ImportProgressText = "Import completed";
                                if (result.Success)
                                {
                                    ImportResultMessage = result.Message;
                                    ImportResultColor = Brushes.Green;
                                    CurrentPage = 1;
                                    LoadAvailableYears();
                                    LoadHistoricalGames();
                                }
                                else
                                {
                                    ImportResultMessage = result.Message;
                                    ImportResultColor = Brushes.Red;
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                IsImporting = false;
                                ImportResultMessage = $"Import error: {ex.Message}";
                                ImportResultColor = Brushes.Red;
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                IsImporting = false;
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}