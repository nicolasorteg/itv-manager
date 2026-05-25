using System.Windows;
using Serilog;

namespace Manager.Views.Splash;

public partial class SplashWindow {
    private CancellationTokenSource _cts = new();

    public SplashWindow() {
        InitializeComponent();
        Loaded += OnWindowLoaded;
    }

    private async void OnWindowLoaded(object sender, RoutedEventArgs e) {
        try {
            // animacion barra progreso
            for (var i = 0; i <= 100; i++) {
                if (_cts.Token.IsCancellationRequested) return;

                MiProgressBar.Value = i;
                await Task.Delay(25, _cts.Token); // bajar ms para más velocidad
            }

            // al terminar la carga se cierra la ventana
            DialogResult = true; 
            Close(); 
        }
        catch (OperationCanceledException) {
            Log.Warning("El user canceló la ejecución del programa. Shutdown...");
            Application.Current.Shutdown();
        }
    }

    /// <summary> Ejecuta un shutdown a la aplicacion </summary>
    private void BtnCancelar_Click(object sender, RoutedEventArgs e) {
        _cts.Cancel();
        Application.Current.Shutdown();
    }
}