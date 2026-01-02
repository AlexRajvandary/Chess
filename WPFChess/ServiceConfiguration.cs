using Microsoft.Extensions.DependencyInjection;
using ChessWPF.Services;
using ChessWPF.ViewModels;

namespace ChessWPF
{
    public static class ServiceConfiguration
    {
        public static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ChessGameService>();
            services.AddSingleton<SoundService>();
            services.AddSingleton<GameStorageService>();
            services.AddSingleton<PanelManagementViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<HistoricalGamesViewModel>();
            services.AddSingleton<TimerViewModel>();
            services.AddSingleton<CapturedPiecesViewModel>();
            services.AddSingleton<MoveHistoryViewModel>();
            services.AddSingleton<GameStorageViewModel>();
            services.AddSingleton<GameViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddTransient<MainWindow>();
            return services.BuildServiceProvider();
        }
    }
}
