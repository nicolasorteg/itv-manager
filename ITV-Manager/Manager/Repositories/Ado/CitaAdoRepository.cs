using System.IO;
using CSharpFunctionalExtensions;
using Manager.Config;
using Manager.Errors.Common;
using Manager.Factories;
using Manager.Models;
using Manager.Repositories.Base;
using Microsoft.Data.Sqlite;
using Serilog;

namespace Manager.Repositories.Ado;

public class CitaAdoRepository: ICitaRepository {
    
    private readonly ILogger _logger = Log.ForContext<CitaAdoRepository>(); // para que en logs se vea la ubicacion
    private readonly string _connectionString = AppConfig.ConnectionString;
    
    // constructor vacío llama al segundo pasando la configuración
    public CitaAdoRepository() : this(AppConfig.DropData, AppConfig.SeedData) { }

    // inicialización de la cadena de conexión
    public CitaAdoRepository(bool dropData, bool seedData) {
        _logger.Debug("Iniciando repository Ado para Citas...");
        
        EnsureDataFolder();
        EnsureTable(dropData);
        
        if (!seedData || CountCita(true) != 0) return;
        _logger.Debug("Sembrando datos iniciales de citas...");
        var citas = CitaFactory.Seed();
        foreach (var cita in citas) { Create(cita); }
    }

    private void EnsureDataFolder() {
        
        // extrae el path del archivo de la cadena de conexión
        var builder = new SqliteConnectionStringBuilder(_connectionString);
        var dbPath = builder.DataSource;

        // si no hay conexion o esta en memoria salimos
        if (string.IsNullOrEmpty(dbPath) || dbPath == ":memory:") return;
        var directory = Path.GetDirectoryName(dbPath); // obtiene unicamente la ruta del directorio
        
        // si el directorio es válido y todavía no existe en el sistema de archivos
        if (string.IsNullOrEmpty(directory) || Directory.Exists(directory)) return;
        
        _logger.Debug($"Creando directorio para la base de datos en: {directory} ...");
        Directory.CreateDirectory(directory); // creacion fisica
    }

    private void EnsureTable(bool dropData) {
        
        // abrimos la conexion con el archivo sqlite
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        
        // drop si la tabla existía (util para limpiar ejecuciones de tests por ejemplo)
        if (dropData) {
            _logger.Warning("DropData activo: Eliminando tabla Citas si existía...");
            using var dropComand = new SqliteCommand("DROP TABLE IF EXISTS Citas;", connection);
            dropComand.ExecuteNonQuery();
        }
        
        var createTableSql = @"
            CREATE TABLE IF NOT EXISTS Citas (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Matricula TEXT NOT NULL,
                Marca TEXT NOT NULL,
                Modelo TEXT NOT NULL,
                Cilindrada INTEGER NOT NULL,
                Motor INTEGER NOT NULL,
                Dni TEXT NOT NULL,
                FechaMatriculacion TEXT NOT NULL,
                FechaInspeccion TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                IsDeleted INTEGER NOT NULL DEFAULT 0,
                DeletedAt TEXT
            );";
        using var createCommand = new SqliteCommand(createTableSql, connection); // compila el comando
        createCommand.ExecuteNonQuery(); // ejecuta el comando en la bd
    }

    
    
    public Cita? GetById(int id) {
        throw new NotImplementedException();
    }

    public Result<Cita, DomainError> Create(Cita entity) {
        throw new NotImplementedException();
    }

    public Result<Cita, DomainError> Update(int id, Cita entity) {
        throw new NotImplementedException();
    }

    public Cita? Delete(int id, bool isLogical = true) {
        throw new NotImplementedException();
    }

    public bool DeleteAll() {
        throw new NotImplementedException();
    }

    public Result<Cita, DomainError> Restore(int id) {
        throw new NotImplementedException();
    }

    public Cita? GetByMatricula(string matricula) {
        throw new NotImplementedException();
    }

    public bool ExisteCitaParaVehiculoEnFecha(string matricula, DateTime fechaInspeccion) {
        throw new NotImplementedException();
    }

    public int ContarCitasPropietarioEnFecha(string dni, DateTime fechaInspeccion) {
        throw new NotImplementedException();
    }

    public int CountCita(bool incluirEliminados = false) {
        throw new NotImplementedException();
    }

    public Result<IEnumerable<Cita>, DomainError> GetWithFilters(DateTime fechaInicio, DateTime? fechaFin, int pagina, int tamPagina, string? searchText = null,
        string motorSeleccionado = "todos", bool incluirEliminados = false) {
        throw new NotImplementedException();
    }

    public int CountCitasFiltradas(string? searchText, DateTime fechaInicio, DateTime? fechaFin, bool incluirEliminados,
        string motorSeleccionado = "todos") {
        throw new NotImplementedException();
    }
    
    
    // métodos auxiliares
    
    /// <summary> Mapea un SqliteDataReader a Cita </summary>
    private Cita MapReaderToCita(SqliteDataReader reader) {
        return new Cita {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Matricula = reader.GetString(reader.GetOrdinal("Matricula")),
            Marca = reader.GetString(reader.GetOrdinal("Marca")),
            Modelo = reader.GetString(reader.GetOrdinal("Modelo")),
            Cilindrada = reader.GetInt32(reader.GetOrdinal("Cilindrada")),
            Motor = (Cita.TiposMotor)reader.GetInt32(reader.GetOrdinal("Motor")),
            Dni = reader.GetString(reader.GetOrdinal("Dni")),
            FechaMatriculacion = DateTime.Parse(reader.GetString(reader.GetOrdinal("FechaMatriculacion"))),
            FechaInspeccion = DateTime.Parse(reader.GetString(reader.GetOrdinal("FechaInspeccion"))),
            IsDeleted = reader.GetInt32(reader.GetOrdinal("IsDeleted")) == 1,
            DeletedAt = reader.IsDBNull(reader.GetOrdinal("DeletedAt")) 
                ? null 
                : DateTime.Parse(reader.GetString(reader.GetOrdinal("DeletedAt")))
        };
    }
}