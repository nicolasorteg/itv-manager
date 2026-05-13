using CSharpFunctionalExtensions;
using Manager.Errors.Common;
using Manager.Models;
using Manager.Validators.Common;
using Manager.Validators.Citas;

namespace Manager.Validators.Citas;

public class CitaValidator: IValidator<Cita> {
    public Result<Cita, DomainError> Validar(Cita cita) {
        
        // listado de mensajes de error
        var errores = new List<string>();
        
        if (!cita.Matricula.IsValidMatricula()) 
            errores.Add("La matrícula no cumple el formato (4 números y 3 letras sin vocales).");
        
        
    }
}