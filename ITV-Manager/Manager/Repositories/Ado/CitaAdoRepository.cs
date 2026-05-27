using System.IO;
using CSharpFunctionalExtensions;
using Manager.Config;
using Manager.Entity;
using Manager.Errors.Citas;
using Manager.Errors.Common;
using Manager.Factories;
using Manager.Mapper;
using Manager.Models;
using Manager.Repositories.Base;
using Microsoft.Data.Sqlite;
using Serilog;

namespace Manager.Repositories.Ado;

/// <summary> Repositorio que usa Ado.NET para gestionar la BD </summary>
public class CitaAdoRepository : ICitaRepository {

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
        foreach (var cita in citas) {
            Create(cita);
        }
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
                Id INTEGER PRIMARY KEY,
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

    /// <inheritdoc cref="ICitaRepository.GetById" />
    public Cita? GetById(int id) {
        
        _logger.Debug($"Obteniendo cita para id: {id}");
        
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Citas WHERE Id = @Id AND IsDeleted = @IsDeleted;";
        command.Parameters.AddWithValue("@Id", id); // asignamos el id
        command.Parameters.AddWithValue("@IsDeleted", 0);
        
        using var reader = command.ExecuteReader();
        return reader.Read() ? MapReaderToEntity(reader).ToModel() : null;
    }

    /// <inheritdoc cref="ICitaRepository.GetAll" />
    public IEnumerable<Cita> GetAll(int pagina = 1, int tamPagina = 10, bool incluirEliminados = false) {
        
        var lista = new List<Cita>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        const string sql = @"
            SELECT * FROM Citas 
            WHERE (@IncludeDeleted = 1 OR IsDeleted = 0)
            ORDER BY Id ASC 
            LIMIT @Limit OFFSET @Offset";

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.AddWithValue("@IncludeDeleted", incluirEliminados ? 1 : 0);
        command.Parameters.AddWithValue("@Limit", tamPagina);
        command.Parameters.AddWithValue("@Offset", (pagina - 1) * tamPagina);

        using var reader = command.ExecuteReader();
        while (reader.Read()) {
            lista.Add(MapReaderToEntity(reader).ToModel()!);
        }

        return lista;
    }
    

    /// <inheritdoc cref="ICitaRepository.Create" />
    public Result<Cita, DomainError> Create(Cita entity) {
        
        _logger.Debug($"Insertando nueva cita para vehículo: {entity.Matricula}");
        
        if (ExisteCitaParaVehiculoEnFecha(entity.Matricula, entity.FechaInspeccion)) {
            _logger.Warning($"Validación fallida: El vehículo {entity.Matricula} ya tiene una cita el día {entity.FechaInspeccion:yyyy-MM-dd}");
            return Result.Failure<Cita, DomainError>(CitaErrors.Database($"El vehículo con matrícula {entity.Matricula} ya tiene una cita asignada para ese día."));
        }
        
        if (ContarCitasPropietarioEnFecha(entity.Dni, entity.FechaInspeccion) >= AppConfig.MaxVehiculosPorDni) {
            _logger.Warning($"Validación fallida: El propietario con DNI {entity.Dni} ya supera el límite de {AppConfig.MaxVehiculosPorDni} citas el día {entity.FechaInspeccion:yyyy-MM-dd}");
            return Result.Failure<Cita, DomainError>(CitaErrors.Database($"El propietario con DNI {entity.Dni} no puede registrar más de {AppConfig.MaxVehiculosPorDni} citas el mismo día."));
        }
        
        var diasHastaInspeccion = (entity.FechaInspeccion.Date - DateTime.Today).TotalDays;
        if (diasHastaInspeccion > AppConfig.VentanaDiasCita) {
            _logger.Warning($"La cita {entity.Matricula} se sale de la ventana de dias para inspeccion. Máximo desde hoy {AppConfig.VentanaDiasCita} días.");
            return Result.Failure<Cita, DomainError>(CitaErrors.Database($"La cita {entity.Matricula} se sale de la ventana de dias para inspeccion. Máximo desde hoy {AppConfig.VentanaDiasCita} días."));
        }

        // cita -> citaentity
        var dbEntity = entity.ToEntity();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        if (entity.Id == 0) {
            // sin id sqlite autogen
            command.CommandText = @"
            INSERT INTO Citas (Matricula, Marca, Modelo, Cilindrada, Motor, Dni, FechaMatriculacion, FechaInspeccion, CreatedAt, UpdatedAt, IsDeleted)
            VALUES (@Matricula, @Marca, @Modelo, @Cilindrada, @Motor, @Dni, @FechaMatriculacion, @FechaInspeccion, @CreatedAt, @UpdatedAt, @IsDeleted);
            SELECT last_insert_rowid();";
            AddParameters(command, dbEntity, incluirId: false);
        } else {
            // con id explicito para seed
            command.CommandText = @"
            INSERT INTO Citas (Id, Matricula, Marca, Modelo, Cilindrada, Motor, Dni, FechaMatriculacion, FechaInspeccion, CreatedAt, UpdatedAt, IsDeleted)
            VALUES (@Id, @Matricula, @Marca, @Modelo, @Cilindrada, @Motor, @Dni, @FechaMatriculacion, @FechaInspeccion, @CreatedAt, @UpdatedAt, @IsDeleted);
            SELECT last_insert_rowid();";
            AddParameters(command, dbEntity, incluirId: true);
        }
        
        // actualiza el modelo original
        entity = entity with { Id = Convert.ToInt32(command.ExecuteScalar()) };

        _logger.Information($"Cita guardada correctamente con ID asignado: {entity.Id}");
    
        return Result.Success<Cita, DomainError>(entity);
    }

    /// <inheritdoc cref="ICitaRepository.Update" />
    public Result<Cita, DomainError> Update(int id, Cita entity) {
        _logger.Debug($"Actualizando cita con ID: {id}");
    
        // si existe la cita
        var existente = GetById(id);
        if (existente == null) {
            return Result.Failure<Cita, DomainError>(CitaErrors.NotFound($"No existe la cita con ID {id} para actualizar."));
        }
        
        // a diferencia del create, aqui verificamos si los campos criticos cambian, es decir si la fecha de inspeccion cambio, dni o matricula
        if (entity.Matricula != existente.Matricula || entity.FechaInspeccion.Date != existente.FechaInspeccion.Date) {
            if (ExisteCitaParaVehiculoEnFecha(entity.Matricula, entity.FechaInspeccion)) {
                _logger.Warning($"Validación fallida en Update: El vehículo {entity.Matricula} ya tiene otra cita el día {entity.FechaInspeccion:yyyy-MM-dd}");
                return Result.Failure<Cita, DomainError>(CitaErrors.Database($"No se puede actualizar: El vehículo con matrícula {entity.Matricula} ya tiene otra cita asignada para ese día."));
            }
        }
        
        if (entity.Dni != existente.Dni || entity.FechaInspeccion.Date != existente.FechaInspeccion.Date) {
            if (ContarCitasPropietarioEnFecha(entity.Dni, entity.FechaInspeccion) >= AppConfig.MaxVehiculosPorDni) {
                _logger.Warning($"Validación fallida en Update: El propietario {entity.Dni} ya tiene {AppConfig.MaxVehiculosPorDni} citas el día {entity.FechaInspeccion:yyyy-MM-dd}");
                return Result.Failure<Cita, DomainError>(CitaErrors.Database($"No se puede actualizar: El propietario con DNI {entity.Dni} ya tiene el límite de {AppConfig.MaxVehiculosPorDni} citas asignadas para ese día."));
            }
        }
        
        // actualiza updateAt
        var entidadActualizada = entity with { Id = id, CreatedAt = existente.CreatedAt, UpdatedAt = DateTime.Now }; // al usar el with es necesario guardar la fecha de creacion para que no se sobreescriba
        var dbEntity = entidadActualizada.ToEntity();

        try {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
            UPDATE Citas SET 
                Matricula = @Matricula, Marca = @Marca, Modelo = @Modelo, 
                Cilindrada = @Cilindrada, Motor = @Motor, Dni = @Dni, 
                FechaMatriculacion = @FechaMatriculacion, FechaInspeccion = @FechaInspeccion, 
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;";
        
            AddParameters(command, dbEntity, incluirId: true);
            command.ExecuteNonQuery();

            return Result.Success<Cita, DomainError>(entidadActualizada);
        }
        catch (Exception ex) {
            _logger.Error(ex, $"Error al actualizar la cita {id}");
            return Result.Failure<Cita, DomainError>(CitaErrors.Database(ex.Message));
        }
    }

    /// <inheritdoc cref="ICitaRepository.Delete" />
    public Cita? Delete(int id, bool isLogical = true) {
        
        _logger.Debug($"Eliminando cita {id}. Borrado lógico: {isLogical}");
        var cita = GetById(id); // busqueda de cita a eliminar
        if (cita == null) return null;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();

        // distincion de tipos de borrado
        if (isLogical) {
            command.CommandText = "UPDATE Citas SET IsDeleted = 1, DeletedAt = @DeletedAt WHERE Id = @Id;";
            command.Parameters.AddWithValue("@DeletedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        } else {
            command.CommandText = "DELETE FROM Citas WHERE Id = @Id;";
        }
        command.Parameters.AddWithValue("@Id", id);
        command.ExecuteNonQuery();

        return cita;
    }

    /// <inheritdoc cref="ICitaRepository.DeleteAll" />
    public bool DeleteAll() {
        
        _logger.Warning("Eliminando físicamente todos los registros de la tabla Citas...");
        try {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = new SqliteCommand("DELETE FROM Citas;", connection);
            command.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al vaciar la tabla Citas");
            return false;
        }
    }

    /// <inheritdoc cref="ICitaRepository.Restore" />
    public Result<Cita, DomainError> Restore(int id) {
        
        _logger.Debug($"Restaurando cita borrada con ID: {id}");
        
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
    
        // bsuqueda cita (no se usa getbyid pq si esta borrada no la mostra´ra)
        Cita? cita = null;
        using (var findCmd = connection.CreateCommand()) {
            findCmd.CommandText = "SELECT * FROM Citas WHERE Id = @Id;";
            findCmd.Parameters.AddWithValue("@Id", id);
            
            using var reader = findCmd.ExecuteReader();
            if (reader.Read()) {
                cita = MapReaderToEntity(reader).ToModel();
            }
        }
        if (cita == null) return Result.Failure<Cita, DomainError>(CitaErrors.Database("No se puede restaurar una Cita que no existe."));
        
        // si no estaba borrada devolvemos success directamente
        if (!cita.IsDeleted) {
            return Result.Success<Cita, DomainError>(cita);
        }

        
        
        // validacion apra no restaurar una cita que rompa la RN-05 (misma matrícula en la misma fecha)
        using var checkMatriculaCmd = connection.CreateCommand();
        checkMatriculaCmd.CommandText = "SELECT COUNT(1) FROM Citas WHERE Matricula = @Matricula AND date(FechaInspeccion) = date(@Fecha) AND IsDeleted = 0 AND Id <> @Id;";
        checkMatriculaCmd.Parameters.AddWithValue("@Matricula", cita.Matricula);
        checkMatriculaCmd.Parameters.AddWithValue("@Fecha", cita.FechaInspeccion.ToString("yyyy-MM-dd"));
        checkMatriculaCmd.Parameters.AddWithValue("@Id", id);
    
        if (Convert.ToInt32(checkMatriculaCmd.ExecuteScalar()) > 0) {
            return Result.Failure<Cita, DomainError>(CitaErrors.Database("No se puede restaurar: El vehículo ya cuenta con otra cita activa ese mismo día."));
        }

        // validacion apra no restaurar una cita que rompa la RN-06 (limite de vehiculos diario por dni)
        using var checkDniCmd = connection.CreateCommand();
        checkDniCmd.CommandText = "SELECT COUNT(1) FROM Citas WHERE Dni = @Dni AND date(FechaInspeccion) = date(@Fecha) AND IsDeleted = 0 AND Id <> @Id;";
        checkDniCmd.Parameters.AddWithValue("@Dni", cita.Dni);
        checkDniCmd.Parameters.AddWithValue("@Fecha", cita.FechaInspeccion.ToString("yyyy-MM-dd"));
        checkDniCmd.Parameters.AddWithValue("@Id", id);

        if (Convert.ToInt32(checkDniCmd.ExecuteScalar()) >= AppConfig.MaxVehiculosPorDni) {
            return Result.Failure<Cita, DomainError>(CitaErrors.Database($"No se puede actualizar: El propietario con DNI {cita.Dni} ya tiene el límite de {AppConfig.MaxVehiculosPorDni} citas asignadas para ese día."));
        }
        
        using var restoreCommand = connection.CreateCommand();
        restoreCommand.CommandText = "UPDATE Citas SET IsDeleted = 0, DeletedAt = NULL, UpdatedAt = @UpdatedAt WHERE Id = @Id;";
        restoreCommand.Parameters.AddWithValue("@Id", id);
        restoreCommand.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        restoreCommand.ExecuteNonQuery();
        
        return Result.Success<Cita, DomainError>(GetById(id)!);
    }

    /// <inheritdoc cref="ICitaRepository.GetByMatricula" />
    public Cita? GetByMatricula(string matricula) {
        
        _logger.Debug($"Obteniendo cita para matricula: {matricula}");
        
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Citas WHERE Matricula = @Matricula AND IsDeleted = 0;";
        command.Parameters.AddWithValue("@Matricula", matricula);

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapReaderToEntity(reader).ToModel() : null;
    }

    /// <inheritdoc cref="ICitaRepository.ExisteCitaParaVehiculoEnFecha" />
    public bool ExisteCitaParaVehiculoEnFecha(string matricula, DateTime fechaInspeccion) {
        
        _logger.Debug($"Comprobando si ya existe cita para la matrícula {matricula} en la fecha {fechaInspeccion:yyyy-MM-dd}");
        
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        
        command.CommandText = "SELECT COUNT(*) FROM Citas WHERE Matricula = @Matricula AND date(FechaInspeccion) = date(@Fecha) AND IsDeleted = 0;";
        command.Parameters.AddWithValue("@Matricula", matricula);
        command.Parameters.AddWithValue("@Fecha", fechaInspeccion.ToString("yyyy-MM-dd"));

        return Convert.ToInt32(command.ExecuteScalar()) > 0; // si es > 0 significa que hay sí que existe
    }

    /// <inheritdoc cref="ICitaRepository.ContarCitasPropietarioEnFecha" />
    public int ContarCitasPropietarioEnFecha(string dni, DateTime fechaInspeccion) {
        
        _logger.Debug($"Contando citas del propietario {dni} en la fecha {fechaInspeccion:yyyy-MM-dd}");
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Citas WHERE Dni = @Dni AND date(FechaInspeccion) = date(@Fecha) AND IsDeleted = 0;";
        command.Parameters.AddWithValue("@Dni", dni);
        command.Parameters.AddWithValue("@Fecha", fechaInspeccion.ToString("yyyy-MM-dd"));

        return Convert.ToInt32(command.ExecuteScalar());
    }

    /// <inheritdoc cref="ICitaRepository.CountCita" />
    public int CountCita(bool incluirEliminados = false) {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = incluirEliminados
            ? "SELECT COUNT(*) FROM Citas;"
            : "SELECT COUNT(*) FROM Citas WHERE IsDeleted = 0;";

        return Convert.ToInt32(command.ExecuteScalar());
    }
    
    /// <inheritdoc cref="ICitaRepository.GetWithFilters" />
    public Result<IEnumerable<Cita>, DomainError> GetWithFilters(DateTime fechaInicio, DateTime? fechaFin, int pagina, int tamPagina, string? searchText = null, string motorSeleccionado = "todos", bool incluirEliminados = false) {

        _logger.Debug("Ejecutando consulta de Citas con filtros...");
        var citas = new List<Cita>();

        try {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();

            // parse motor a numerico para la bd
            var motorValue = -1;
            if (!motorSeleccionado.Equals("todos", StringComparison.OrdinalIgnoreCase)) { // si no pone todos
                if (string.Equals(motorSeleccionado, "gasolina", StringComparison.OrdinalIgnoreCase)) motorValue = 0;
                else if (string.Equals(motorSeleccionado, "diesel", StringComparison.OrdinalIgnoreCase))
                    motorValue = 1;
                else if (string.Equals(motorSeleccionado, "electrico", StringComparison.OrdinalIgnoreCase))
                    motorValue = 2;
                else if (string.Equals(motorSeleccionado, "hibrido", StringComparison.OrdinalIgnoreCase))
                    motorValue = 3;
            }
            
            command.CommandText = @"
            SELECT * FROM Citas 
            WHERE date(FechaInspeccion) >= date(@FechaInicio)
              AND (@FechaFin IS NULL OR date(FechaInspeccion) <= date(@FechaFin))
              AND (@IncluirEliminados = 1 OR IsDeleted = 0)
              AND (@MotorText = 'todos' OR Motor = @MotorInt)
              AND (@Search IS NULL OR (
                  LOWER(Matricula) LIKE @Search OR 
                  LOWER(Dni) LIKE @Search OR 
                  LOWER(Marca) LIKE @Search OR 
                  LOWER(Modelo) LIKE @Search
              ))
            ORDER BY Id ASC
            LIMIT @Limit OFFSET @Offset;";

            // mapeo parametros
            command.Parameters.AddWithValue("@FechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@FechaFin", fechaFin.HasValue ? fechaFin.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            command.Parameters.AddWithValue("@IncluirEliminados", incluirEliminados ? 1 : 0);
            command.Parameters.AddWithValue("@MotorText", motorSeleccionado.ToLower());
            command.Parameters.AddWithValue("@MotorInt", motorValue);
            command.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(searchText) ? DBNull.Value : $"%{searchText.ToLower()}%");
            command.Parameters.AddWithValue("@Limit", tamPagina);
            command.Parameters.AddWithValue("@Offset", (pagina - 1) * tamPagina);
            
            
            using var reader = command.ExecuteReader();
            while (reader.Read()) {
                citas.Add(MapReaderToEntity(reader).ToModel()!);
            }
            
            return Result.Success<IEnumerable<Cita>, DomainError>(citas);
            
        } catch (Exception ex) {
            _logger.Error(ex, "Error en la consulta con filtros");
            return Result.Failure<IEnumerable<Cita>, DomainError>(CitaErrors.Database(ex.Message));
        }
    }

    /// <inheritdoc cref="ICitaRepository.CountCitasFiltradas" />
    public int CountCitasFiltradas(string? searchText, DateTime fechaInicio, DateTime? fechaFin, bool incluirEliminados, string motorSeleccionado = "todos") {
        
        var motorValue = -1;
        if (!motorSeleccionado.Equals("todos", StringComparison.OrdinalIgnoreCase)) {
            if (string.Equals(motorSeleccionado, "gasolina", StringComparison.OrdinalIgnoreCase)) motorValue = 0;
            else if (string.Equals(motorSeleccionado, "diesel", StringComparison.OrdinalIgnoreCase))
                motorValue = 1;
            else if (string.Equals(motorSeleccionado, "electrico", StringComparison.OrdinalIgnoreCase))
                motorValue = 2;
            else if (string.Equals(motorSeleccionado, "hibrido", StringComparison.OrdinalIgnoreCase))
                motorValue = 3;
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();

        // misma consulta que GetWithFilters pero sin limitacion de paginacion
        command.CommandText = @"
            SELECT COUNT(*) FROM Citas 
                WHERE date(FechaInspeccion) >= date(@FechaInicio)
                  AND (@FechaFin IS NULL OR date(FechaInspeccion) <= date(@FechaFin))
                  AND (@IncluirEliminados = 1 OR IsDeleted = 0)
                  AND (@MotorText = 'todos' OR Motor = @MotorInt)
                  AND (@Search IS NULL OR (
                      LOWER(Matricula) LIKE @Search OR 
                      LOWER(Dni) LIKE @Search OR 
                      LOWER(Marca) LIKE @Search OR 
                      LOWER(Modelo) LIKE @Search
                  ))";

        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@FechaFin", fechaFin.HasValue ? fechaFin.Value.ToString("yyyy-MM-dd") : DBNull.Value);
        command.Parameters.AddWithValue("@IncluirEliminados", incluirEliminados ? 1 : 0);
        command.Parameters.AddWithValue("@MotorText", motorSeleccionado.ToLower());
        command.Parameters.AddWithValue("@MotorInt", motorValue);
        command.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(searchText) ? DBNull.Value : $"%{searchText.ToLower()}%");

        return Convert.ToInt32(command.ExecuteScalar());
    }


    // métodos auxiliares

    /// <summary> Mapea un SqliteDataReader a Cita </summary>
    private static CitaEntity MapReaderToEntity(SqliteDataReader reader) {
        return new CitaEntity {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Matricula = reader.GetString(reader.GetOrdinal("Matricula")),
            Marca = reader.GetString(reader.GetOrdinal("Marca")),
            Modelo = reader.GetString(reader.GetOrdinal("Modelo")),
            Cilindrada = reader.GetInt32(reader.GetOrdinal("Cilindrada")),
            Motor = reader.GetInt32(reader.GetOrdinal("Motor")),
            Dni = reader.GetString(reader.GetOrdinal("Dni")),
            FechaMatriculacion = DateTime.Parse(reader.GetString(reader.GetOrdinal("FechaMatriculacion"))),
            FechaInspeccion = DateTime.Parse(reader.GetString(reader.GetOrdinal("FechaInspeccion"))),
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
            UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt"))),
            IsDeleted = reader.GetInt32(reader.GetOrdinal("IsDeleted")) == 1,
            DeletedAt = reader.IsDBNull(reader.GetOrdinal("DeletedAt"))
                ? null
                : DateTime.Parse(reader.GetString(reader.GetOrdinal("DeletedAt")))
        };
    }

    /// <summary> Funcion para ahorrar codigo añadiendo parametros a las consultas </summary>
    private static void AddParameters(SqliteCommand command, CitaEntity entity, bool incluirId = false) {
        if (incluirId) command.Parameters.AddWithValue("@Id", entity.Id);
        command.Parameters.AddWithValue("@Matricula", entity.Matricula);
        command.Parameters.AddWithValue("@Marca", entity.Marca);
        command.Parameters.AddWithValue("@Modelo", entity.Modelo);
        command.Parameters.AddWithValue("@Cilindrada", entity.Cilindrada);
        command.Parameters.AddWithValue("@Motor", entity.Motor);
        command.Parameters.AddWithValue("@Dni", entity.Dni);
        command.Parameters.AddWithValue("@FechaMatriculacion", entity.FechaMatriculacion.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@FechaInspeccion", entity.FechaInspeccion.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@IsDeleted", entity.IsDeleted ? 1 : 0);
    }
}