using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace ChessWPF
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _serviceProvider = ServiceConfiguration.ConfigureServices();
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
