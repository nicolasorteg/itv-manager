using System.Diagnostics;
using System.Windows;
using Manager.Config;
using Manager.Infrastructure;
using Manager.Service.Backup;
using Manager.Service.ImportExport;
using Manager.Service.Manager;
using Manager.Service.Report;
using Manager.ViewModels.Main;
using Manager.Views.Main;
using Manager.Views.Splash;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Debugging;

namespace Manager;

public partial class App {
    private static IServiceProvider ServiceProvider { get; set; } = null!;

    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        ConfigureSerilog();
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
            var importExportService = ServiceProvider.GetRequiredService<IImportExportService>();
            var backupService = ServiceProvider.GetRequiredService<IBackupService>();
            var mainViewModel = new MainViewModel(managerService, reportService, importExportService, backupService);

            
            var mainWindow = new MainWindow { DataContext = mainViewModel };
            
            mainWindow.Show(); // mostrar ventana main
        } 
        else { Shutdown(); } // si se cancela la carga
    }
    
    private static void ConfigureSerilog() {
        SelfLog.Enable(msg => Debug.WriteLine($"SERILOG DIAG: {msg}"));

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(AppConfig.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        Log.Information("Sistema de logs configurado desde el appsettings.json");
    }
    
    /// <summary>  Se ejecuta al cerrar la aplicación </summary>
    protected override void OnExit(ExitEventArgs e) {
        Log.Information("Aplicación cerrándose...");
        Log.CloseAndFlush();
        if (ServiceProvider is IDisposable disposable) disposable.Dispose();
        base.OnExit(e);
    }
}