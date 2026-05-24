using CSharpFunctionalExtensions;
using Manager.Errors.Common;
using Manager.Models;

namespace Manager.Service.ImportExport;

public interface IImportExportService {
    
    /// <summary> Importa citas de un archivo </summary>
    Result<IEnumerable<Cita>, DomainError> ImportarDatos(string path);
    
    /// <summary> Exporta citas a un archivo </summary>
    Result<int, DomainError> ExportarDatos(IEnumerable<Cita> citas, string path);

    /// <summary> Importa personas desde la ruta por defecto </summary>
    Result<IEnumerable<Cita>, DomainError> ImportarDatosSistema(string path);
    
    /// <summary> Exporta citas usando el directorio default </summary>
    Result<int, DomainError> ExportarDatosSistema(IEnumerable<Cita> citas);
}