using System.Windows;

namespace Manager.Views.Splash;

public partial class SplashWindow : Window {
    private CancellationTokenSource _cts = new CancellationTokenSource();

    public SplashWindow() {
        InitializeComponent();
        Loaded += OnWindowLoaded;
    }

    private async void OnWindowLoaded(object sender, RoutedEventArgs e) {
        try {
            // Animación de la barra de progreso (Mantiene la UI fluida)
            for (int i = 0; i <= 100; i++) {
                if (_cts.Token.IsCancellationRequested) return;

                MiProgressBar.Value = i;
                await Task.Delay(35, _cts.Token); // 35ms por tick para que cargue rápido y elegante
            }

            // Al terminar la carga, cerramos la ventana de Splash
            this.DialogResult = true; 
            this.Close(); 
        }
        catch (OperationCanceledException) {
            Application.Current.Shutdown();
        }
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e) {
        _cts.Cancel();
        Application.Current.Shutdown();
    }
}