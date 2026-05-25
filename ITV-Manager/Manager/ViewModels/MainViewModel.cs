using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Manager.Config;
using Manager.Models;
using Manager.Service.Manager;
using Manager.Service.Report;
using Microsoft.Win32;

namespace Manager.ViewModels;

public class MainViewModel : INotifyPropertyChanged {
    private readonly IManagerService _managerService;
    private readonly IReportService _reportService;
    private ObservableCollection<Cita> _citas = new();
    
    private int _paginaActual = 1;
    private const int TamPagina = 10;
    private int _totalPaginas = 1;

    public ObservableCollection<Cita> Citas {
        get => _citas;
        set { _citas = value; OnPropertyChanged(); }
    }

    public int PaginaActual {
        get => _paginaActual;
        set { _paginaActual = value; OnPropertyChanged(); }
    }

    public int TotalPaginas {
        get => _totalPaginas;
        set { _totalPaginas = value; OnPropertyChanged(); }
    }

    public ICommand PaginaAnteriorCommand { get; }
    public ICommand PaginaSiguienteCommand { get; }
    public ICommand ImportarCommand { get; }
    public ICommand ExportarCommand { get; }
    public ICommand MostrarDetallesCommand { get; }
    public ICommand MostrarAcercaDeCommand { get; }
    public ICommand ActualizarCitaCommand { get; }
    public ICommand ExportarCitaCommand { get; }
    public ICommand BorrarCitaCommand { get; }
    
    private Cita? _citaSeleccionada;
    
    public Cita? CitaSeleccionada {
        get => _citaSeleccionada;
        set { _citaSeleccionada = value; OnPropertyChanged(); }
    }
    public MainViewModel(IManagerService managerService, IReportService reportService) {
        _managerService = managerService;
        _reportService = reportService;

        PaginaAnteriorCommand = new RelayCommand(_ => CambiarPagina(-1), _ => PaginaActual > 1);
        PaginaSiguienteCommand = new RelayCommand(_ => CambiarPagina(1), _ => PaginaActual < TotalPaginas);
        
        ImportarCommand = new RelayCommand(_ => MessageBox.Show("Funcionalidad de Importar próximamente.", "Importar", MessageBoxButton.OK, MessageBoxImage.Information));
        ExportarCommand = new RelayCommand(_ => MessageBox.Show("Funcionalidad de Exportar próximamente.", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information));
        
        MostrarDetallesCommand = new RelayCommand(_ => EjecutarMostrarDetalles());
        MostrarAcercaDeCommand = new RelayCommand(_ => AbriVentanaAcercaDe());
        
        ActualizarCitaCommand = new RelayCommand(_ => EjecutarActualizar(), _ => CitaSeleccionada != null);
        ExportarCitaCommand = new RelayCommand(_ => EjecutarExportarCita(), _ => CitaSeleccionada != null);
        BorrarCitaCommand = new RelayCommand(_ => EjecutarBorradoLogico(), _ => CitaSeleccionada != null);
        
        CargarCitas();
    }

    private void CargarCitas() {
        // 1. Usamos ObtenerConFiltros pasándole los parámetros requeridos por tu arquitectura
        // Usamos DateTime.MinValue como inicio para que abarque cualquier registro histórico activo
        var resultado = _managerService.ObtenerConFiltros(
            fechaInicio: DateTime.MinValue, 
            fechaFin: null, 
            pagina: PaginaActual, 
            tamPagina: TamPagina,
            searchText: null,
            motorSeleccionado: "todos",
            incluirEliminados: false // FALSO: No queremos ver registros borrados lógicamente
        );

        if (resultado.IsSuccess) {
            // Volcamos los registros filtrados directamente a la lista que lee el DataGrid de WPF
            Citas = new ObservableCollection<Cita>(resultado.Value);
        } else {
            Citas = new ObservableCollection<Cita>();
            MessageBox.Show($"Error al cargar el listado de citas", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        // 2. CÁLCULO DINÁMICO DE PAGINACIÓN:
        // Le pedimos al servicio el conteo real exacto de registros en BD que cumplen la condición de estar activos
        int totalRegistrosActivos = _managerService.ContarCitasFiltradas(
            searchText: null,
            fechaInicio: DateTime.MinValue,
            fechaFin: null,
            motorSeleccionado: "todos",
            incluirEliminados: false
        ); 
    
        // Calculamos el total de páginas en base a los registros reales (ej. 18 / 10 = 2 páginas)
        TotalPaginas = (int)Math.Ceiling((double)totalRegistrosActivos / TamPagina);
    
        if (TotalPaginas < 1) TotalPaginas = 1; 
    }
    private void CambiarPagina(int direccion) {
        PaginaActual += direccion;
        CargarCitas();
    }

    private void EjecutarMostrarDetalles() {
        string mensaje = $"=== CONFIGURACIÓN DEL SISTEMA ===\n\n" +
                         $"• Motor de BD Activo: {AppConfig.RepositoryType.ToUpper()}\n" +
                         $"• Formato Almacenamiento: {AppConfig.StorageType.ToUpper()}\n" +
                         $"• Borrado Lógico Activado: Sí (IsDeleted = 1)\n" +
                         $"• Sembrado de Datos (Seed): {(AppConfig.SeedData ? "Activado" : "Desactivado")}\n" +
                         $"• Límite Diario por Propietario: {AppConfig.MaxVehiculosPorDni} vehículos\n" +
                         $"• Versión del Framework: .NET 8.0 WPF";
                         
        MessageBox.Show(mensaje, "Detalles del Proyecto", MessageBoxButton.OK, MessageBoxImage.Asterisk);
    }

    private void AbriVentanaAcercaDe() {
        // Al estar separados, invocamos a la nueva ventana limpia que crearemos abajo
        var acercaDeWin = new Views.About.AboutWindow {
            Owner = Application.Current.MainWindow
        };
        acercaDeWin.ShowDialog();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private void EjecutarActualizar() {
        if (CitaSeleccionada == null) return;
        MessageBox.Show($"Formulario para modificar la matrícula {CitaSeleccionada.Matricula} próximamente.", "Actualizar Cita", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void EjecutarExportarCita() {
        if (CitaSeleccionada == null) return;

        // 1. Preguntar al usuario mediante un diálogo de opciones corporativo
        var result = MessageBox.Show(
            "¿Desea exportar la ficha en formato PDF?\n\n(Pulse 'Sí' para PDF, o 'No' para descargar la plantilla en HTML plano)", 
            "Seleccionar Formato de Ficha", 
            MessageBoxButton.YesNoCancel, 
            MessageBoxImage.Question
        );

        if (result == MessageBoxResult.Cancel) return;

        // 2. Configurar el explorador nativo SaveFileDialog para guardar archivos
        SaveFileDialog saveFileDialog = new SaveFileDialog {
            FileName = $"FichaITV_{CitaSeleccionada.Matricula}",
            InitialDirectory = AppConfig.ReportDirectory
        };

        if (result == MessageBoxResult.Yes) {
            // Flujo para PDF
            saveFileDialog.Filter = "Documento PDF (*.pdf)|*.pdf";
            saveFileDialog.DefaultExt = "pdf";

            if (saveFileDialog.ShowDialog() == true) {
                var resPdf = _reportService.ExportarCitaAPdf(CitaSeleccionada);
                if (resPdf.IsSuccess) {
                    // Mover el temporal generado por el servicio a la ruta elegida por el usuario
                    if (File.Exists(resPdf.Value)) {
                        File.Move(resPdf.Value, saveFileDialog.FileName, overwrite: true);
                    }
                    MessageBox.Show($"¡Ficha PDF guardada correctamente en:\n{saveFileDialog.FileName}", "Exportación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        } 
        else if (result == MessageBoxResult.No) {
            // Flujo para HTML
            saveFileDialog.Filter = "Archivo Web HTML (*.html)|*.html";
            saveFileDialog.DefaultExt = "html";

            if (saveFileDialog.ShowDialog() == true) {
                var resHtml = _reportService.ExportarCitaAHtml(CitaSeleccionada);
                if (resHtml.IsSuccess) {
                    if (File.Exists(resHtml.Value)) {
                        File.Move(resHtml.Value, saveFileDialog.FileName, overwrite: true);
                    }
                    MessageBox.Show($"¡Ficha HTML generada correctamente en:\n{saveFileDialog.FileName}", "Exportación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }

    private void EjecutarBorradoLogico() {
        if (CitaSeleccionada == null) return;
    
        var confirmacion = MessageBox.Show(
            $"¿Seguro que deseas dar de baja la cita con matrícula {CitaSeleccionada.Matricula}?", 
            "Confirmar Borrado", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Warning
        );

        if (confirmacion == MessageBoxResult.Yes) {
            var resultadoBorrado = _managerService.EliminarCita(CitaSeleccionada.Id, esLogico: true);

            if (resultadoBorrado.IsSuccess) {
                MessageBox.Show("Registro eliminado de la vista activa.", "Operación Completada", MessageBoxButton.OK, MessageBoxImage.Information);
            
                // Si borras el último elemento de la página 2, volvemos automáticamente a la página 1
                if (Citas.Count == 1 && PaginaActual > 1) {
                    PaginaActual--;
                }
            
                CargarCitas(); // Esto volverá a ejecutar el GetAll actualizado y el conteo dinámico
            }
        }
    }
}

public class RelayCommand : ICommand {
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null) {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);
    public void Execute(object? parameter) => _execute(parameter);
    public event EventHandler? CanExecuteChanged {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}