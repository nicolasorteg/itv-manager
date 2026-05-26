using System.IO;
using System.IO.Compression;
using CSharpFunctionalExtensions;
using Manager.Config;
using Manager.Errors.Common;
using Manager.Errors.Storage;
using Manager.Models;
using Manager.Storage.Common;
using Serilog;

namespace Manager.Service.Backup;

public class BackupService(IStorage<Cita> storage, string? defaultBackupDirectory = null) : IBackupService {
    
    private readonly ILogger _logger = Log.ForContext<BackupService>();

    /// <inheritdoc cref="IBackupService.RealizarBackup" />
    public Result<string, DomainError> RealizarBackup(IEnumerable<Cita> citas, string? customDirectory = null) {
        var backDir = customDirectory ?? defaultBackupDirectory ?? AppConfig.BackupDirectory;
        _logger.Information("Iniciando backup en: {dir}", backDir);

        var lista = citas.ToList();
        if (lista.Count == 0) {
            _logger.Warning("No hay datos para respaldar.");
            return Result.Failure<string, DomainError>(StorageErrors.WriteError("No hay datos para respaldar."));
        }

        try {
            Directory.CreateDirectory(backDir);
        }
        catch (Exception ex) {
            return Result.Failure<string, DomainError>(StorageErrors.WriteError($"No se pudo crear el directorio: {ex.Message}"));
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"backup-{Guid.NewGuid()}");
        var dataDir = Path.Combine(tempDir, "data");
        Directory.CreateDirectory(dataDir);

        try {
            var jsonPath = Path.Combine(dataDir, "citas.json");
            var writeResult = storage.WriteToFile(lista, jsonPath);
            if (writeResult.IsFailure) {
                return Result.Failure<string, DomainError>(StorageErrors.WriteError("Error al serializar los datos."));
            }

            var fecha = DateTime.Now.ToString("yyyyMMdd-HH-mm-ss");
            var zipPath = Path.Combine(backDir, $"{fecha}-backup.zip");
            ZipFile.CreateFromDirectory(tempDir, zipPath);

            _logger.Information("Backup creado: {zipPath}", zipPath);
            return Result.Success<string, DomainError>(zipPath);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al crear el backup");
            return Result.Failure<string, DomainError>(StorageErrors.WriteError(ex.Message));
        }
        finally {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
    
    /// <inheritdoc cref="IBackupService.RestaurarBackup" />
    public Result<IEnumerable<Cita>, DomainError> RestaurarBackup(string archivoBackup) {
        _logger.Information("Restaurando desde: {archivo}", archivoBackup);

        if (!File.Exists(archivoBackup))
            return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.ArchivoNoEncontrado(archivoBackup));

        var tempDir = Path.Combine(Path.GetTempPath(), $"restore-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try {
            ZipFile.ExtractToDirectory(archivoBackup, tempDir);

            var jsonPath = Path.Combine(tempDir, "data", "citas.json");
            return !File.Exists(jsonPath) ? Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.ArchivoNoEncontrado("citas.json")) : storage.ReadFromFile(jsonPath);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al restaurar el backup");
            return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.ReadError(ex.Message));
        }
        finally {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    /// <inheritdoc cref="IBackupService.ListarBackups" />
    public IEnumerable<string> ListarBackups(string? customDirectory = null) {
        var backDir = customDirectory ?? defaultBackupDirectory ?? AppConfig.BackupDirectory;
        if (!Directory.Exists(backDir)) return [];
        return Directory.GetFiles(backDir, "*.zip").OrderByDescending(File.GetCreationTime);
    }
}