using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using Manager.Config;
using Manager.Dto;
using Manager.Errors.Common;
using Manager.Errors.Storage;
using Manager.Mapper;
using Manager.Models;
using Manager.Storage.Common;
using Serilog;

namespace Manager.Storage.Json;

public class CitaJsonStorage : ICitaJsonStorage {
    
    private readonly ILogger _logger = Log.ForContext<CitaJsonStorage>();

    // config
    private readonly JsonSerializerOptions _options = new() {
        WriteIndented = true, 
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, 
        Converters = { new JsonStringEnumConverter() }, 
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
    };
    public CitaJsonStorage() : this(AppConfig.DataFolder) { }
    public CitaJsonStorage(string dataFolder) {
        _logger.Debug("Inicializando la clase CitaJsonStorage");
        InitStorage(dataFolder);
    }
    
    /// <inheritdoc cref="IStorage{T}.WriteToFile" />
    public Result<bool, DomainError> WriteToFile(IEnumerable<Cita> citas, string path) {
        try {
            _logger.Debug("Guardando las citas en el archivo JSON...");
            
            // tranforma a dtos
            var jsonDtos = citas.Select(c => c.ToDto()).ToList();
            
            // serializacion
            var json = JsonSerializer.Serialize(jsonDtos, _options);
            
            File.WriteAllText(path, json, new UTF8Encoding(false)); // write al archivo
            
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al guardar el archivo JSON en '{path}'", path);
            return Result.Failure<bool, DomainError>(StorageErrors.WriteError(ex.Message));
        }
    }
    
    /// <inheritdoc cref="IStorage{T}.ReadFromFile" />
    public Result<IEnumerable<Cita>, DomainError> ReadFromFile(string path) {
        _logger.Debug("Cargando citas desde el archivo JSON...");
        
        if (!File.Exists(path)) {
            _logger.Warning("El archivo JSON no existe");
            return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.ArchivoNoEncontrado(path));
        }

        try {
            //lectura
            var json = File.ReadAllText(path, Encoding.UTF8);
            
            var dtos = JsonSerializer.Deserialize<List<CitaDto>>(json, _options); // deserializa

            // si está corrupto
            if (dtos == null) {
                return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.FormatoInvalido("No se pudieron deserializar los DTO de Citas."));
            }

            // map a model
            var modelos = dtos.Select(dto => dto.ToModel()).ToList();

            return Result.Success<IEnumerable<Cita>, DomainError>(modelos);
        }
        catch (JsonException ex) {
            _logger.Error(ex, "Error en el formato estructural del archivo JSON");
            return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.FormatoInvalido(ex.Message));
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error genérico de lectura en el archivo JSON");
            return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.ReadError(ex.Message));
        }
    }
    
    private void InitStorage(string folder) {
        if (Directory.Exists(folder)) return; // si ya existe sale
        _logger.Debug("El directorio no existe. Creándolo...");
        Directory.CreateDirectory(folder); // si no, lo crea
    }
}