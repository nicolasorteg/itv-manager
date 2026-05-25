using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Manager.Config;
using Manager.Service.Manager;
using Manager.Service.Report;
using Manager.ViewModels.Citas;
using Manager.ViewModels.Informe;

namespace Manager.ViewModels.Main;

public partial class MainViewModel : ObservableObject {
    public CitasViewModel CitasVM { get; }
    public InformeViewModel InformeVM { get; }

    public MainViewModel(IManagerService managerService, IReportService reportService) {
        // subviewmodels
        CitasVM = new CitasViewModel(managerService);
        InformeVM = new InformeViewModel(reportService);
        CitasVM.CargarCitas();
    }

    [RelayCommand]
    private void ImportarGlobal() {
        MessageBox.Show("Funcionalidad de Importar próximamente.", "Importar", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ExportarGlobal() {
        MessageBox.Show("Funcionalidad de Exportar próximamente.", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void MostrarDetalles() {
        var mensaje = $"-----⚙️ CONFIGURACIÓN DEL SISTEMA ⚙️-----\n\n" +
                      $"• Motor de BD Activo: {AppConfig.RepositoryType.ToUpper()}\n" +
                      $"• Formato Almacenamiento: {AppConfig.StorageType.ToUpper()}\n" +
                      $"• Borrado Lógico Activado: {AppConfig.UseLogicalDelete}\n" +
                      $"• Sembrado de Datos (Seed): {(AppConfig.SeedData ? "Activado" : "Desactivado")}\n" +
                      $"• Límite Diario por Propietario: {AppConfig.MaxVehiculosPorDni} vehículos\n" +
                      $"• Versión del Framework: {AppConfig.Version}";
                         
        MessageBox.Show(mensaje, "Detalles del Proyecto", MessageBoxButton.OK, MessageBoxImage.Asterisk);
    }

    [RelayCommand]
    private void MostrarAcercaDe() {
        var acercaDeWin = new Views.About.AboutWindow {
            Owner = Application.Current.MainWindow
        };
        acercaDeWin.ShowDialog();
    }
}