using CSharpFunctionalExtensions;
using Manager.Errors.Common;

namespace Manager.Validators.Common;

/// <summary> Contrato para la validación de entidades </summary>
/// <typeparam name="T">Entidad a validar</typeparam>
public interface IValidator<T> {
    
    /// <summary> Valida una entidad </summary>
    /// <param name="entity">Entidad a validar</param>
    /// <returns>
    /// - La entidad si la validación ha sido un éxito.
    /// - Error de dominio si no.
    /// </returns>
    Result<T, DomainError> Validar(T entity);
}