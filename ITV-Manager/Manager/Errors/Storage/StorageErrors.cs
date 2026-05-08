using Manager.Errors.Common;

namespace Manager.Errors.Storage;

/// <summary> Factory para crear errores del Almacenamiento </summary>
public static class StorageErrors {

     /// <inheritdoc cref="StorageError.ArchivoNoEncontrado"/>
     public static DomainError ArchivoNoEncontrado(string ruta) => 
          new StorageError.ArchivoNoEncontrado(ruta);

     /// <inheritdoc cref="StorageError.FormatoInvalido"/>
     public static DomainError FormatoInvalido(string detalles) => 
          new StorageError.FormatoInvalido(detalles);

     /// <inheritdoc cref="StorageError.WriteError"/>
     public static DomainError WriteError(string detalles) => 
          new StorageError.WriteError(detalles);

     /// <inheritdoc cref="StorageError.ReadError"/>
     public static DomainError ReadError(string detalles) => 
          new StorageError.ReadError(detalles);

     /// <inheritdoc cref="StorageError.AccesoError"/>
     public static DomainError AccesoError(string detalles) => 
          new StorageError.AccesoError(detalles);
}