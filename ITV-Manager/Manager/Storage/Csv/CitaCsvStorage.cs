using System.IO;
using System.Text;
using CSharpFunctionalExtensions;
using Manager.Config;
using Manager.Errors.Common;
using Manager.Errors.Storage;
using Manager.Mapper;
using Manager.Models;
using Manager.Storage.Common;
using Serilog;

namespace Manager.Storage.Csv;

public class CitaCsvStorage : ICitaCsvStorage {
    
    private readonly ILogger _logger = Log.ForContext<CitaCsvStorage>();
    private const string Cabecera = "Id;Matricula;Dni;Marca;Modelo;Cilindrada;Motor;FechaMatriculacion;FechaInspeccion;CreatedAt;UpdatedAt;IsDeleted;DeletedAt";
    
    // constructores
    public CitaCsvStorage() : this(AppConfig.DataFolder) { }
    public CitaCsvStorage(string dataFolder) {
        _logger.Debug("Inicializando la clase CitaCsvStorage");
        InitStorage(dataFolder); // data
    }
    
    /// <inheritdoc cref="IStorage{T}.WriteToFile" />
    public Result<bool, DomainError> WriteToFile(IEnumerable<Cita> citas, string path) {
        try {
            _logger.Debug("Guardando las citas en el archivo CSV...");
            using var writer = new StreamWriter(path, false, new UTF8Encoding(false));
            
            // escritura cabecera
            writer.WriteLine(Cabecera);

            // parseamos cada cita a DTO y la escribimos en el fichero
            foreach (var cita in citas) {
                var dto = cita.ToDto();
                writer.WriteLine(
                    $"{dto.Id};" +
                    $"{EscapeCsvField(dto.Matricula)};" +
                    $"{EscapeCsvField(dto.Dni)};" +
                    $"{EscapeCsvField(dto.Marca)};" +
                    $"{EscapeCsvField(dto.Modelo)};" +
                    $"{dto.Cilindrada};" +
                    $"{EscapeCsvField(dto.Motor)};" +
                    $"{dto.FechaMatriculacion};" +
                    $"{dto.FechaInspeccion};" +
                    $"{dto.CreatedAt};" +
                    $"{dto.UpdatedAt};" +
                    $"{dto.IsDeleted};" +
                    $"{EscapeCsvField(dto.DeletedAt ?? "")}"
                );
            }
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al guardar el CSV");
            return Result.Failure<bool, DomainError>(StorageErrors.WriteError(ex.Message));
        }
    }
    
    /// <inheritdoc cref="IStorage{T}.ReadFromFile" />
    public Result<IEnumerable<Cita>, DomainError> ReadFromFile(string path) {
        
        _logger.Debug("Cargando citas desde el archivo CSV...");

        // si no existe el archivo
        if (!File.Exists(path)) {
            _logger.Warning("El archivo CSV no existe.");
            return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.ArchivoNoEncontrado(path));
        }
        
        try {
            // lectura archivo
            var citas = File.ReadLines(path, Encoding.UTF8)
                .Skip(1) // cabecera
                .Where(linea => !string.IsNullOrWhiteSpace(linea)) 
                .Select(linea => linea.Split(';')) // campos spliteados con ;
                .Select(campos => new Dto.CitaDto(
                    int.Parse(campos[0]),
                    campos[1],
                    campos[2],
                    campos[3],
                    campos[4],
                    int.Parse(campos[5]),
                    campos[6],
                    campos[7],
                    campos[8],
                    campos[9],
                    campos[10],
                    bool.TryParse(campos[11], out var isDel) && isDel,
                    string.IsNullOrEmpty(campos[12]) ? null : campos[12]
                ).ToModel()) // conversion de datos
                .ToList();

            return Result.Success<IEnumerable<Cita>, DomainError>(citas);
        }
        catch (Exception ex) {
            _logger.Error("Error al procesar o parsear el formato del archivo CSV");
            return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.FormatoInvalido(ex.Message));
        }
    }
    
    private static string EscapeCsvField(string field) {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(';') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
    
    private void InitStorage(string folder) {
        if (Directory.Exists(folder)) return; // si ya existe sale
        _logger.Debug("El directorio no existe. Creándolo...");
        Directory.CreateDirectory(folder); // si no, lo crea
    }
}