using CSharpFunctionalExtensions;
using Manager.Errors.Common;
using Manager.Models;

namespace Manager.Service.Backup;

/// <summary> Contrato para la gestión de backups </summary>
public interface IBackupService {
    
    /// <summary> Realiza un backup de las Citas existentes en un ZIp </summary>
    Result<string, DomainError> RealizarBackup(IEnumerable<Cita> citas, string? customDirectory = null);
    
    /// <summary> Restaura un backup desde un archivo ZIP </summary>
    Result<IEnumerable<Cita>, DomainError> RestaurarBackup(string archivoBackup);
    
    /// <summary> Lista los archivos de backup disponibles en el directorio </summary>
    IEnumerable<string> ListarBackups(string? customDirectory = null);
}