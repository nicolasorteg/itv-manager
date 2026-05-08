using Manager.Errors.Common;

namespace Manager.Errors.Storage;

/// <summary> Contenedor de errores específicos para el Almacenamiento </summary>
/// <param name="Mensaje">Mensaje de error</param>
public abstract record StorageError(string Mensaje) : DomainError(Mensaje) {
    
    /// <summary> Error de archivo no encontrado </summary>
    /// <param name="Path">Ruta completa del archivo</param>
    public sealed record ArchivoNoEncontrado(string Path)
        : StorageError($"No se ha encontrado el archivo en la ruta: {Path}");

    /// <summary> Error cuando el formato del archivo es inválido </summary>
    /// <param name="Detalles">Información sobre el error de formato</param>
    public sealed record FormatoInvalido(string Detalles)
        : StorageError($"El formato del archivo es inválido o incompatible. {Detalles}");

    /// <summary> Error en la escritura </summary>
    /// <param name="Detalles">Información sobre el error de formato</param>
    public sealed record WriteError(string Detalles)
        : StorageError($"Error al escribir en el almacenamiento. {Detalles}");

    /// <summary> Error en la lectura </summary>
    /// <param name="Detalles">Información sobre el error de formato</param>
    public sealed record ReadError(string Detalles)
        : StorageError($"Error al leer del almacenamiento. {Detalles}");

    /// <summary> Error en el acceso al archivo </summary>
    /// <param name="Detalles">Información sobre el error de formato</param>
    public sealed record AccesoError(string Detalles)
        : StorageError($"Error de acceso al almacenamiento. {Detalles}");
}