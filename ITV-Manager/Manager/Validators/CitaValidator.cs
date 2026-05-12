using CSharpFunctionalExtensions;
using Manager.Errors.Common;
using Manager.Models;
using Manager.Validators.Common;


namespace Manager.Validators;

public class CitaValidator: IValidator<Cita> {
    public Result<Cita, DomainError> Validar(Cita cita) {
        throw new NotImplementedException();
    }
}