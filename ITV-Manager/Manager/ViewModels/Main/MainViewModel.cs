using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Manager.Config;
using Manager.Service.Backup;
using Manager.Service.ImportExport;
using Manager.Service.Manager;
using Manager.Service.Report;
using Manager.ViewModels.Citas;
using Manager.ViewModels.Informe;

namespace Manager.ViewModels.Main;

public partial class MainViewModel : ObservableObject {
    public CitasViewModel CitasVM { get; }
    public InformeViewModel InformeVM { get; }
    
    private readonly IManagerService _managerService;
    private readonly IImportExportService _importExportService;
    private readonly IBackupService _backupService;

    public MainViewModel(IManagerService managerService, IReportService reportService, IImportExportService importExportService, IBackupService backupService) {
        // subviewmodels
        _managerService = managerService;
        _importExportService = importExportService;
        CitasVM = new CitasViewModel(managerService);
        InformeVM = new InformeViewModel(reportService);
        _backupService = backupService; 
        CitasVM.CargarCitas();
    }

    [RelayCommand]
    private void ImportarGlobal() {
        var dialog = new Microsoft.Win32.OpenFileDialog {
            Title = "Importar citas",
            Filter = "Archivos de datos|*.json;*.xml;*.csv|JSON|*.json|XML|*.xml|CSV|*.csv"
        };

        if (dialog.ShowDialog() != true) return;

        var resultado = _importExportService.ImportarDatos(dialog.FileName);
        if (resultado.IsFailure) {
            MessageBox.Show($"Error al importar: {resultado.Error.Mensaje}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // vaciar e insertar nuevas
        _managerService.EliminarTodasLasCitas();
        var citas = resultado.Value.ToList();
        int ok = 0, fail = 0;
        foreach (var cita in citas) {
            var r = _managerService.CrearCita(cita);
            if (r.IsSuccess) ok++; else fail++;
        }

        CitasVM.CargarCitas();
        var msg = $"Importación completada: {ok} citas añadidas.";
        if (fail > 0) msg += $"\n{fail} citas omitidas por reglas de negocio.";
        MessageBox.Show(msg, "Importación", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ExportarGlobal() {
        var extension = AppConfig.StorageType.ToLower();
        var filtro = extension switch {
            "xml" => "XML|*.xml",
            "csv" => "CSV|*.csv",
            _     => "JSON|*.json"
        };

        var dialog = new Microsoft.Win32.SaveFileDialog {
            Title = "Exportar citas",
            Filter = filtro,
            FileName = $"citas_export_{DateTime.Today:yyyyMMdd}"
        };

        if (dialog.ShowDialog() != true) return;

        // obtener todas incluidas las eliminadas
        var todasLasCitas = _managerService.ObtenerTodas(1, int.MaxValue, incluirEliminados: true);
        var resultado = _importExportService.ExportarDatos(todasLasCitas, dialog.FileName);

        if (resultado.IsSuccess) {
            MessageBox.Show($"Exportación completada: {resultado.Value} citas exportadas.", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
        } else {
            MessageBox.Show($"Error al exportar: {resultado.Error.Mensaje}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
    
    [RelayCommand]
    private void RealizarBackup() {
        var todas = _managerService.ObtenerTodas(1, int.MaxValue, incluirEliminados: true);
        var resultado = _backupService.RealizarBackup(todas);
        if (resultado.IsSuccess)
            MessageBox.Show($"Backup creado:\n{resultado.Value}", "Backup", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show($"Error: {resultado.Error.Mensaje}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}