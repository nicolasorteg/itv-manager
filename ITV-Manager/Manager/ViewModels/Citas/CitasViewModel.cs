using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Manager.Models;
using Manager.Service.Manager;

namespace Manager.ViewModels.Citas;

public partial class CitasViewModel(IManagerService managerService) : ObservableObject {
    
    private readonly IManagerService _managerService = managerService;
    private const int TamPagina = 10;

    [ObservableProperty] private ObservableCollection<Cita> _citas = new();
    [ObservableProperty] private int _paginaActual = 1;
    [ObservableProperty] private int _totalPaginas = 1;
    [ObservableProperty] private Cita? _citaSeleccionada;
    [ObservableProperty] private string _busquedaId = "";
    [ObservableProperty] private string _busquedaMatricula = "";

    // Métodos parciales nativos del Toolkit que reaccionan automáticamente a los cambios
    partial void OnCitaSeleccionadaChanged(Cita? value) {
        ActualizarCitaCommand.NotifyCanExecuteChanged();
        BorrarCitaCommand.NotifyCanExecuteChanged();
    }

    partial void OnPaginaActualChanged(int value) {
        CargarCitas();
    }

    public void CargarCitas() {
        var resultado = _managerService.ObtenerConFiltros(
            fechaInicio: DateTime.MinValue, 
            fechaFin: null, 
            pagina: PaginaActual, 
            tamPagina: TamPagina,
            searchText: null,
            motorSeleccionado: "todos",
            incluirEliminados: false
        );

        if (resultado.IsSuccess) {
            Citas = new ObservableCollection<Cita>(resultado.Value);
        } else {
            Citas = new ObservableCollection<Cita>();
            MessageBox.Show("Error al cargar el listado de citas", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        var totalRegistrosActivos = _managerService.ContarCitasFiltradas(
            searchText: null, fechaInicio: DateTime.MinValue, fechaFin: null, motorSeleccionado: "todos", incluirEliminados: false
        ); 

        TotalPaginas = (int)Math.Ceiling((double)totalRegistrosActivos / TamPagina);
        if (TotalPaginas < 1) TotalPaginas = 1; 

        // Notificar el estado de los botones de paginación
        PaginaAnteriorCommand.NotifyCanExecuteChanged();
        PaginaSiguienteCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanPrevia))]
    private void PaginaAnterior() {
        if (PaginaActual > 1) PaginaActual--;
    }
    private bool CanPrevia() => PaginaActual > 1;

    [RelayCommand(CanExecute = nameof(CanSiguiente))]
    private void PaginaSiguiente() {
        if (PaginaActual < TotalPaginas) PaginaActual++;
    }
    private bool CanSiguiente() => PaginaActual < TotalPaginas;

    [RelayCommand(CanExecute = nameof(CanOperarCita))]
    private void ActualizarCita() {
        if (CitaSeleccionada == null) return;

        var editVM = new EditarCitaViewModel(CitaSeleccionada, _managerService);
        var editWindow = new Views.Citas.EditarCitaWindow {
            DataContext = editVM,
            Owner = Application.Current.MainWindow
        };

        if (editWindow.ShowDialog() == true) {
            CargarCitas();
        }
    }

    [RelayCommand(CanExecute = nameof(CanOperarCita))]
    private void BorrarCita() {
        if (CitaSeleccionada == null) return;
    
        var confirmacion = MessageBox.Show(
            $"¿Seguro que deseas dar de baja la cita con matrícula {CitaSeleccionada.Matricula}?", 
            "Confirmar Borrado", MessageBoxButton.YesNo, MessageBoxImage.Warning
        );

        if (confirmacion != MessageBoxResult.Yes) return;
        var resultadoBorrado = _managerService.EliminarCita(CitaSeleccionada.Id, esLogico: true);

        if (!resultadoBorrado.IsSuccess) return;
        MessageBox.Show("Registro eliminado de la vista activa.", "Operación Completada", MessageBoxButton.OK, MessageBoxImage.Information);
            
        if (Citas.Count == 1 && PaginaActual > 1) {
            PaginaActual--;
        }
            
        CargarCitas();
    }
    
    [RelayCommand]
    private void CrearCita() {
        var editVM = new EditarCitaViewModel(_managerService);
        var editWindow = new Views.Citas.EditarCitaWindow {
            DataContext = editVM,
            Owner = Application.Current.MainWindow
        };

        if (editWindow.ShowDialog() == true) {
            CargarCitas();
        }
    }
    
    [RelayCommand]
    private void BuscarPorId() {
        if (!int.TryParse(BusquedaId.Trim(), out var id)) {
            MessageBox.Show("Introduce un ID numérico válido.", "Búsqueda", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var resultado = _managerService.ObtenerPorId(id);
        if (resultado.IsSuccess) {
            Citas = new ObservableCollection<Cita> { resultado.Value };
            TotalPaginas = 1;
            PaginaAnteriorCommand.NotifyCanExecuteChanged();
            PaginaSiguienteCommand.NotifyCanExecuteChanged();
        } else {
            MessageBox.Show($"No se encontró ninguna cita con ID {id}.", "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void BuscarPorMatricula() {
        if (string.IsNullOrWhiteSpace(BusquedaMatricula)) {
            CargarCitas();
            return;
        }

        var resultado = _managerService.ObtenerPorMatricula(BusquedaMatricula.Trim().ToUpper());
        if (resultado.IsSuccess) {
            Citas = new ObservableCollection<Cita> { resultado.Value };
            TotalPaginas = 1;
            PaginaAnteriorCommand.NotifyCanExecuteChanged();
            PaginaSiguienteCommand.NotifyCanExecuteChanged();
        } else {
            MessageBox.Show($"No se encontró ninguna cita con matrícula {BusquedaMatricula}.", "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private bool CanOperarCita() => CitaSeleccionada != null;
}