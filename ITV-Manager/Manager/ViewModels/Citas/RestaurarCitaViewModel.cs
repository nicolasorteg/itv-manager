using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Manager.Models;
using Manager.Service.Manager;

namespace Manager.ViewModels.Citas;

public partial class RestaurarCitaViewModel : ObservableObject {

    private readonly IManagerService _managerService;

    [ObservableProperty] private ObservableCollection<Cita> _citasEliminadas = new();
    [ObservableProperty] private Cita? _citaSeleccionada;

    partial void OnCitaSeleccionadaChanged(Cita? value) =>
        RestaurarCommand.NotifyCanExecuteChanged();

    public RestaurarCitaViewModel(IManagerService managerService) {
        _managerService = managerService;
        CargarEliminadas();
    }

    private void CargarEliminadas() {
        var todas = _managerService.ObtenerTodas(1, int.MaxValue, incluirEliminados: true);
        var eliminadas = todas.Where(c => c.IsDeleted).ToList();
        CitasEliminadas = new ObservableCollection<Cita>(eliminadas);
    }

    [RelayCommand(CanExecute = nameof(CanRestaurar))]
    private void Restaurar() {
        if (CitaSeleccionada == null) return;

        var resultado = _managerService.RestaurarCita(CitaSeleccionada.Id);
        if (resultado.IsSuccess) {
            MessageBox.Show($"Cita {CitaSeleccionada.Matricula} restaurada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            CitasEliminadas.Remove(CitaSeleccionada);
            CitaSeleccionada = null;
        } else {
            MessageBox.Show($"No se pudo restaurar: {resultado.Error.Mensaje}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool CanRestaurar() => CitaSeleccionada != null;

    [RelayCommand]
    private static void Cerrar(Window ventana) => ventana.Close();
}