using CSharpFunctionalExtensions;
using Manager.Errors.Citas;
using Manager.Errors.Common;
using Manager.Models;
using Manager.Validators.Common;
using Serilog;

namespace Manager.Validators.Citas;

/// <summary>
/// Validador para campos de Cita. Se usa el Result del ROP.
/// </summary>
public class CitaValidator: IValidator<Cita> {
    
    /// <inheritdoc cref="IValidator{T}.Validar" />
    public Result<Cita, DomainError> Validar(Cita cita) {
        Log.Debug($"🔵 Validando cita de ID: {cita.Id}");
        
        // listado de mensajes de error
        var errores = new List<string>();
        
        // validacion campo por campo con funciones de extensión
        if (!cita.Matricula.IsValidMatricula()) 
            errores.Add("La matrícula no cumple el formato (NNNNLLL).");
        
        if (!cita.Dni.IsValidDni()) 
            errores.Add("El DNI no es válido o la letra de control es incorrecta.");
        
        if (!cita.Marca.IsValidMarca()) 
            errores.Add("La marca es obligatoria y debe tener menos de 15 caracteres.");

        if (!cita.Modelo.IsValidModelo()) 
            errores.Add("El modelo es obligatorio y debe tener menos de 15 caracteres.");

        if (!cita.Cilindrada.IsValidCilindrada()) 
            errores.Add("La cilindrada debe estar entre 1 y 9000 cc.");

        if (!cita.Motor.IsValidMotor()) 
            errores.Add("El tipo de motor seleccionado no es válido para el sistema.");

        if (!cita.FechaMatriculacion.IsValidFechaMatriculacion()) 
            errores.Add("La fecha de matriculación no puede ser una fecha futura.");

        if (!cita.FechaInspeccion.IsValidFechaInspeccion()) 
            errores.Add("La fecha de inspección debe ser desde hoy hasta el límite configurado.");
        
        // si es true significa que hay algún error, devuelve fallo
        // si es false no habrá ningún error, devuelve exito
        return errores.Any() ? 
            Result.Failure<Cita, DomainError>(CitaErrors.Validation(errores)) : 
            Result.Success<Cita, DomainError>(cita);
    }
}