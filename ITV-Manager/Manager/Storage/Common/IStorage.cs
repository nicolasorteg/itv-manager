using CSharpFunctionalExtensions;
using Manager.Errors.Common;

namespace Manager.Storage.Common;

/// <summary> Contrato genérico para guardado y escritura de objetos </summary>
public interface IStorage<T> {
    
    /// <summary> Escribe una colección de objetos a un archivo </summary>
    Result<bool, DomainError> WriteToFile(IEnumerable<T> citas, string path);
    
    /// <summary> Carga una colección de objetos desde un archivo </summary>
    Result<IEnumerable<T>, DomainError> ReadFromFile(string path);
}