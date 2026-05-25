using System.Windows;
using System.Windows.Input;

namespace Manager.Views.About;

public partial class AboutWindow : Window {
    public AboutWindow() {
        InitializeComponent();
    }

    private void TxtLink_MouseDown(object sender, MouseButtonEventArgs e) {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
            FileName = "https://github.com/nicolasorteg",
            UseShellExecute = true
        });
    }

    private void BtnCerrar_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}