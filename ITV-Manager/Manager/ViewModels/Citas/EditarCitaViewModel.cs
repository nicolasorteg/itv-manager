using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Manager.Models;
using Manager.Service.Manager;

namespace Manager.ViewModels.Citas;

public partial class EditarCitaViewModel : ObservableObject {

    private readonly IManagerService _managerService;
    private readonly int _idOriginal;

    // campos editables
    [ObservableProperty] private string _matricula = "";
    [ObservableProperty] private string _dni = "";
    [ObservableProperty] private string _marca = "";
    [ObservableProperty] private string _modelo = "";
    [ObservableProperty] private int _cilindrada;
    [ObservableProperty] private Cita.TiposMotor _motor;
    [ObservableProperty] private DateTime _fechaMatriculacion;
    [ObservableProperty] private DateTime _fechaInspeccion;

    public bool Guardado { get; private set; } = false;
    public IEnumerable<Cita.TiposMotor> TiposMotor => Enum.GetValues<Cita.TiposMotor>();

    public EditarCitaViewModel(Cita cita, IManagerService managerService) {
        _managerService = managerService;
        _idOriginal = cita.Id;

        // cargar datos actuales
        Matricula = cita.Matricula;
        Dni = cita.Dni;
        Marca = cita.Marca;
        Modelo = cita.Modelo;
        Cilindrada = cita.Cilindrada;
        Motor = cita.Motor;
        FechaMatriculacion = cita.FechaMatriculacion;
        FechaInspeccion = cita.FechaInspeccion;
    }

    [RelayCommand]
    private void Guardar(Window ventana) {
        var citaActualizada = new Cita {
            Id = _idOriginal,
            Matricula = Matricula,
            Dni = Dni,
            Marca = Marca,
            Modelo = Modelo,
            Cilindrada = Cilindrada,
            Motor = Motor,
            FechaMatriculacion = FechaMatriculacion,
            FechaInspeccion = FechaInspeccion
        };

        var resultado = _managerService.ActualizarCita(_idOriginal, citaActualizada);

        if (resultado.IsSuccess) {
            Guardado = true;
            MessageBox.Show("Cita actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            ventana.DialogResult = true;
            ventana.Close();
        } else {
            MessageBox.Show($"Error al actualizar: {resultado.Error.Mensaje}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private static void Cancelar(Window ventana) {
        ventana.DialogResult = false;
        ventana.Close();
    }
}