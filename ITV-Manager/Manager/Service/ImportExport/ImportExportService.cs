using CSharpFunctionalExtensions;
using Manager.Errors.Common;
using Manager.Models;
using Manager.Storage.Common;
using Serilog;

namespace Manager.Service.ImportExport;

/// <summary> Servicio para importar y exportar citas desde las 3 extensiones disponibles </summary>
public class ImportExportService(IStorage<Cita> storage): IImportExportService {
    
    private readonly ILogger _logger = Log.ForContext<ImportExportService>();
    
    /// <inheritdoc cref="IImportExportService.ImportarDatos" />
    public Result<IEnumerable<Cita>, DomainError> ImportarDatos(string path) {
        _logger.Debug($"Importando datos -> {path}");
        return storage.ReadFromFile(path);
    }
    
    /// <inheritdoc cref="IImportExportService.ExportarDatos" />
    public Result<int, DomainError> ExportarDatos(IEnumerable<Cita> citas, string path) {
        _logger.Debug($"Exportando datos -> {path}");
        var lista = citas.ToList();
        return storage.WriteToFile(lista, path)
            .Map(_ => lista.Count);
    }
    
    /// <inheritdoc cref="IImportExportService.ExportarDatosSistema" />
    public Result<int, DomainError> ExportarDatosSistema(IEnumerable<Cita> citas) {
        _logger.Debug("Autoguardado...");
        return ExportarDatos(citas, "");
    }
    
    /// <inheritdoc cref="IImportExportService.ImportarDatosSistema" />
    public Result<IEnumerable<Cita>, DomainError> ImportarDatosSistema(string path) {
        _logger.Debug($"Carga inicial -> {path}");
        return ImportarDatos(path);
    }
}