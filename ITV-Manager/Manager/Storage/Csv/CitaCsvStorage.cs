using System.IO;
using CSharpFunctionalExtensions;
using Manager.Config;
using Manager.Errors.Common;
using Manager.Models;
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
    
    /// <inheritdoc cref="ICitaCsvStorage.Write" />
    public Result<bool, DomainError> Write(IEnumerable<Cita> items, string path) {
        
    }
    
    
    /// <inheritdoc cref="ICitaCsvStorage.Save" />
    public Result<IEnumerable<Cita>, DomainError> Save(string path) {
        throw new NotImplementedException();
    }
    
    private void InitStorage(string folder) {
        if (Directory.Exists(folder)) return; // si ya existe sale
        _logger.Debug("El directorio no existe. Creándolo...");
        Directory.CreateDirectory(folder); // si no, lo crea
    }
}