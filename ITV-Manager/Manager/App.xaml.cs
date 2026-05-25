using System.Windows;
using Manager.Infrastructure;
using Manager.Service.Manager;
using Manager.Service.Report;
using Manager.ViewModels;
using Manager.Views.Main;
using Manager.Views.Splash;
using Microsoft.Extensions.DependencyInjection;

namespace Manager;

public partial class App {
    private static IServiceProvider ServiceProvider { get; set; } = null!;

    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);

        // evita que wpf cierre la aplicacion cuando acabe el splashscreen
        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // inicializamos el contenedor de dependencias
        ServiceProvider = DependenciesProvider.BuildServiceProvider();

        // mostramos el splash
        var splashResult = new SplashWindow().ShowDialog();

        // si el splash se completó con éxito
        if (splashResult == true) {
            
            Current.ShutdownMode = ShutdownMode.OnLastWindowClose; // modo de cierra estándar

            // inicializamos servicios
            var managerService = ServiceProvider.GetRequiredService<IManagerService>();
            var reportService = ServiceProvider.GetRequiredService<IReportService>();
            var mainViewModel = new MainViewModel(managerService, reportService);
            
            var mainWindow = new MainWindow { DataContext = mainViewModel };
            
            mainWindow.Show(); // mostrar ventana main
        } 
        else { Shutdown(); } // si se cancela la carga
    }
}