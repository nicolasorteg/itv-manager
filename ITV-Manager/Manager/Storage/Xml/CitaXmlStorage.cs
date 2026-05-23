using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CSharpFunctionalExtensions;
using Manager.Config;
using Manager.Dto;
using Manager.Errors.Common;
using Manager.Errors.Storage;
using Manager.Mapper;
using Manager.Models;
using Serilog;

namespace Manager.Storage.Xml;

public class CitaXmlStorage : ICitaXmlStorage{
    
    private readonly ILogger _logger = Log.ForContext<CitaXmlStorage>();
    
    private readonly XmlSerializerNamespaces _xmlSerializerNamespaces = new();

    private readonly XmlWriterSettings _xmlWriterSettings = new() {
        Indent = true,
        Encoding = new UTF8Encoding(false),
    };

    public CitaXmlStorage() : this(AppConfig.DataFolder) { }
    public CitaXmlStorage(string dataFolder) {
        _logger.Debug("Inicializando la clase CitaJsonStorage");
        InitStorage(dataFolder);
    }

    /// <inheritdoc cref="ICitaXmlStorage.WriteToFile" />
    public Result<bool, DomainError> WriteToFile(IEnumerable<Cita> citas, string path) {
        try {
            _logger.Debug("Guardando los items en el archivo XML '{path}'", path);
            
            var citaDtos = citas.Select(v => v.ToDto()).ToList();
            var serializer = new XmlSerializer(typeof(List<CitaDto>));

            using var streamWriter = new StreamWriter(path, false, new UTF8Encoding(false));
            using var xmlWriter = XmlWriter.Create(streamWriter, _xmlWriterSettings);
            
            serializer.Serialize(xmlWriter, citaDtos, _xmlSerializerNamespaces);
            
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al escribir las citas en el archivo XML");
            return Result.Failure<bool, DomainError>(StorageErrors.WriteError(ex.Message));
        }
    }
    
    /// <inheritdoc cref="ICitaXmlStorage.ReadFromFile" />
    public Result<IEnumerable<Cita>, DomainError> ReadFromFile(string path) {
        _logger.Debug("Cargando los items del archivo XML");

        if (!File.Exists(path)) {
            _logger.Warning("El archivo XML no existe");
            return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.ArchivoNoEncontrado(path));
        }

        try {
            var serializer = new XmlSerializer(typeof(List<CitaDto>));
            
            using var stream = File.OpenRead(path);
            
            var dtos = serializer.Deserialize(stream) as List<CitaDto>;
            
            // dtos no puede ser nulo ya que en caso de serlo el serializer directamente lanza la excepcion y se atrapa en el catch
            return Result.Success<IEnumerable<Cita>, DomainError>(dtos!.Select(dto => dto.ToModel()));
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al leer las citas del archivo XML");
            return Result.Failure<IEnumerable<Cita>, DomainError>(StorageErrors.AccesoError(ex.Message));
        }
    }
    
    private void InitStorage(string folder) {
        if (Directory.Exists(folder)) return; // si ya existe sale
        _logger.Debug("El directorio no existe. Creándolo...");
        Directory.CreateDirectory(folder); // si no, lo crea
    }
}