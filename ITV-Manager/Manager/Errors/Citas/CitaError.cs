using Manager.Errors.Common;

namespace Manager.Errors.Citas;

/// <summary> Contenedor para los errores específicos para Citas </summary>
/// <param name="Mensaje">Mensaje de error</param>
public abstract record CitaError(string Mensaje) : DomainError(Mensaje) {
    
    /// <summary> Error de Cita no encontrada según ID </summary>
    /// <param name="Id">Identificador único</param>
    public sealed record NotFound(string Id) 
        : CitaError($"No se ha encontrado ninguna cita con el identificador: {Id}");
    
    /// <summary> Error de validación en la Cita </summary>
    /// <param name="Errores">Listado de mensajes de errores de validación</param>
    public sealed record Validation(IEnumerable<string> Errores) 
        : CitaError($"Se han detectado errores de validación en la entidad:{Environment.NewLine} - {string.Join($"{Environment.NewLine}• ", Errores)}");
    
    /// <summary> Error en las Reglas de Negocio - RN-05 (No se permiten varias inspecciones el mismo día) </summary>
    /// <param name="Matricula">Matrícula del coche</param>
    /// <param name="Fecha">Fecha de la cita</param>
    public sealed record InspeccionRepetida(string Matricula, DateTime Fecha) 
        : CitaError($"El vehículo con matrícula {Matricula} ya tiene una cita asignada para el día {Fecha:dd/MM/yyyy}.");
    
    /// <summary> Error en las Reglas de Negocio - RN-06 (Un propietario no puede pasar más de 3 inspecciones en un día) </summary>
    /// <param name="Dni">Dni del propietario</param>
    /// <param name="Fecha">Día exacto</param>
    public sealed record MaximosVehiculosAlcanzados(string Dni, DateTime Fecha) 
        : CitaError($"El DNI {Dni} ya tiene 3 vehículos registrados para el día {Fecha:dd/MM/yyyy}.");
    
    public sealed record Database(string Detalles)
        : CitaError($"Error de base de datos: {Detalles}");
}