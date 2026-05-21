using Manager.Errors.Common;

namespace Manager.Errors.Citas;

/// <summary> Factory para crear errores de Citas </summary>
public static class CitaErrors {

    /// <inheritdoc cref="CitaError.NotFound"/>
    public static DomainError NotFound(string id) =>
        new CitaError.NotFound(id);

    /// <inheritdoc cref="CitaError.Validation"/>
    public static DomainError Validation(IEnumerable<string> errors) =>
        new CitaError.Validation(errors);
    
    /// <inheritdoc cref="CitaError.FechaFueraDeRango"/>
    public static DomainError FechaFueraDeRango(DateTime fecha) =>
        new CitaError.FechaFueraDeRango(fecha);
    
    /// <inheritdoc cref="CitaError.InspeccionRepetida"/>
    public static DomainError InspeccionRepetida(string matricula, DateTime fecha) =>
        new CitaError.InspeccionRepetida(matricula, fecha);

    /// <inheritdoc cref="CitaError.MaximosVehiculosAlcanzados"/>
    public static DomainError MaximosVehiculosAlcanzados(string dni, DateTime fecha) =>
        new CitaError.MaximosVehiculosAlcanzados(dni, fecha);
    
    public static DomainError Database(string detalles) =>
        new CitaError.Database(detalles);
}