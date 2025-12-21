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
        
        public HistoricalGamesViewModel()
        {
            HistoricalGames = new ObservableCollection<HistoricalGame>();
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
                    return "Нет партий";
                return $"{totalCount} партий ({DatabaseSizeFormatted})";
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
                    return "Нет партий";
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                int startIndex = (currentPage - 1) * pageSize + 1;
                int endIndex = Math.Min(currentPage * pageSize, totalCount);
                return $"Страница {currentPage} из {totalPages} ({startIndex}-{endIndex} из {totalCount})";
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
                var (games, totalCount) = GameStorageService.GetHistoricalGamesPaginated(currentPage, pageSize);
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
                MessageBox.Show($"Ошибка при загрузке исторических партий: {ex.Message}", "Ошибка", 
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
                        ImportResultMessage = $"Файл {fileName} уже был импортирован ранее.";
                        ImportResultColor = Brushes.Orange;
                        return;
                    }
                    IsImporting = true;
                    ImportProgress = 0;
                    ImportProgressText = "Начинаем импорт...";
                    ImportResultMessage = "";
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            var result = GameStorageService.ImportPgnFile(filePath, (progress, current, total) =>
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    ImportProgress = progress;
                                    ImportProgressText = $"Импортировано: {current} из {total}";
                                });
                            });
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                IsImporting = false;
                                ImportProgress = 100;
                                ImportProgressText = "Импорт завершен";
                                if (result.Success)
                                {
                                    ImportResultMessage = result.Message;
                                    ImportResultColor = Brushes.Green;
                                    CurrentPage = 1;
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
                                ImportResultMessage = $"Ошибка при импорте: {ex.Message}";
                                ImportResultColor = Brushes.Red;
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                IsImporting = false;
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}