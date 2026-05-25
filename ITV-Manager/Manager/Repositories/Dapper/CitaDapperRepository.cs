using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using Manager.Config;
using Manager.Entity;
using Manager.Errors.Citas;
using Manager.Errors.Common;
using Manager.Errors.Storage;
using Manager.Factories;
using Manager.Mapper;
using Manager.Models;
using Manager.Repositories.Base;
using Serilog;

namespace Manager.Repositories.Dapper;

/// <summary> Repositorio que usa Dapper con SQLite para la BD </summary>
public class CitaDapperRepository: ICitaRepository {
    
    private readonly IDbConnection _connection;
    private readonly ILogger _logger = Log.ForContext<CitaDapperRepository>();
    private readonly Action? _onDispose;

    // constructor
    public CitaDapperRepository(IDbConnection connection, Action? onDispose = null, bool dropData = false, bool seedData = false) {
        _connection = connection;
        _onDispose = onDispose;
        
        EnsureTable(dropData);

        if (!seedData || CountTotal() != 0) return;
        foreach (var cita in CitaFactory.Seed()) Create(cita);
    }
    
    /// <inheritdoc cref="ICitaRepository.GetById" />
    public Cita? GetById(int id) {
        try {
            _logger.Debug($"Obteniendo cita por ID: {id}");
            const string Sql = "SELECT * FROM Citas WHERE Id = @Id";
            var entity = _connection.QueryFirstOrDefault<CitaEntity>(Sql, new { Id = id });
            return entity?.ToModel();
        }
        catch (Exception ex) {
            _logger.Error(ex, $"Error al obtener cita por ID {id}.");
            return null;
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.GetAll" />
    public IEnumerable<Cita> GetAll(int pagina = 1, int tamPagina = 10, bool incluirEliminados = false) {
        try {
            _logger.Debug("Obteniendo todos de forma paginada...");
            
            const string Sql = @"
                SELECT * FROM Citas 
                WHERE (@IncludeDeleted = 1 OR IsDeleted = 0)
                ORDER BY Id ASC
                LIMIT @Limit OFFSET @Offset";

            var entities = _connection.Query<CitaEntity>(Sql, new {
                IncludeDeleted = incluirEliminados ? 1 : 0,
                Limit = tamPagina,
                Offset = (pagina - 1) * tamPagina
            });

            return entities.Select(e => e.ToModel()!);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error en GetAll.");
            return [];
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.Create" />
    public Result<Cita, DomainError> Create(Cita entity) {
        try {
            _logger.Debug($"Insertando nueva cita para vehículo: {entity.Matricula}");

            // validaciones RN
            if (ExisteCitaParaVehiculoEnFecha(entity.Matricula, entity.FechaInspeccion)) {
                _logger.Warning($"Validación fallida: El vehículo {entity.Matricula} ya tiene una cita el día {entity.FechaInspeccion:yyyy-MM-dd}");
                return Result.Failure<Cita, DomainError>(CitaErrors.Database($"El vehículo con matrícula {entity.Matricula} ya tiene una cita asignada para ese día."));
            }

            if (ContarCitasPropietarioEnFecha(entity.Dni, entity.FechaInspeccion) >= AppConfig.MaxVehiculosPorDni) {
                _logger.Warning($"Validación fallida: El propietario con DNI {entity.Dni} ya supera el límite de {AppConfig.MaxVehiculosPorDni} citas");
                return Result.Failure<Cita, DomainError>(CitaErrors.Database($"El propietario con DNI {entity.Dni} no puede registrar más de {AppConfig.MaxVehiculosPorDni} citas el mismo día."));
            }
            
            var diasHastaInspeccion = (entity.FechaInspeccion.Date - DateTime.Today).TotalDays;
            if (diasHastaInspeccion > AppConfig.VentanaDiasCita) {
                _logger.Warning($"La cita {entity.Matricula} se sale de la ventana de dias para inspeccion. Máximo desde hoy {AppConfig.VentanaDiasCita} días.");
                return Result.Failure<Cita, DomainError>(CitaErrors.Database($"La cita {entity.Matricula} se sale de la ventana de dias para inspeccion. Máximo desde hoy {AppConfig.VentanaDiasCita} días."));
            }

            var parametrosBase = new {
                entity.Matricula, entity.Marca, entity.Modelo, entity.Cilindrada,
                Motor = (int)entity.Motor, entity.Dni,
                FechaMatriculacion = entity.FechaMatriculacion.ToString("yyyy-MM-dd"),
                FechaInspeccion = entity.FechaInspeccion.ToString("yyyy-MM-dd HH:mm:ss"),
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                IsDeleted = entity.IsDeleted
            };

            int id;
            if (entity.Id == 0) {
                const string SqlSinId = @"
                    INSERT INTO Citas (Matricula, Marca, Modelo, Cilindrada, Motor, Dni, FechaMatriculacion, FechaInspeccion, CreatedAt, UpdatedAt, IsDeleted)
                    VALUES (@Matricula, @Marca, @Modelo, @Cilindrada, @Motor, @Dni, @FechaMatriculacion, @FechaInspeccion, @CreatedAt, @UpdatedAt, @IsDeleted);
                    SELECT last_insert_rowid();";
                id = _connection.QuerySingle<int>(SqlSinId, parametrosBase);
            } else {
                const string SqlConId = @"
                    INSERT INTO Citas (Id, Matricula, Marca, Modelo, Cilindrada, Motor, Dni, FechaMatriculacion, FechaInspeccion, CreatedAt, UpdatedAt, IsDeleted)
                    VALUES (@Id, @Matricula, @Marca, @Modelo, @Cilindrada, @Motor, @Dni, @FechaMatriculacion, @FechaInspeccion, @CreatedAt, @UpdatedAt, @IsDeleted);
                    SELECT last_insert_rowid();";
                id = _connection.QuerySingle<int>(SqlConId, new { entity.Id, entity.Matricula, entity.Marca, entity.Modelo, entity.Cilindrada, Motor = (int)entity.Motor, entity.Dni,
                    FechaMatriculacion = entity.FechaMatriculacion.ToString("yyyy-MM-dd"),
                    FechaInspeccion = entity.FechaInspeccion.ToString("yyyy-MM-dd HH:mm:ss"),
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsDeleted = entity.IsDeleted });
            }

            _logger.Information($" Cita guardada  correctamente con ID: {id}");
            return Result.Success<Cita, DomainError>(GetById(id)!);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al crear cita.");
            return Result.Failure<Cita, DomainError>(CitaErrors.Database(ex.Message));
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.Update" />
    public Result<Cita, DomainError> Update(int id, Cita entity) {
        _logger.Debug($"Actualizando cita con ID: {id}");

        var existente = GetById(id);
        // comprobacion de que exista la cita a actualizar
        if (existente == null) {
            return Result.Failure<Cita, DomainError>(CitaErrors.NotFound($"No existe la cita con ID {id} para actualizar."));
        }

        // verificaciones RN
        if (entity.Matricula != existente.Matricula || entity.FechaInspeccion.Date != existente.FechaInspeccion.Date) {
            if (ExisteCitaParaVehiculoEnFecha(entity.Matricula, entity.FechaInspeccion)) {
                _logger.Warning($"Validación fallida en Update: El vehículo {entity.Matricula} ya tiene otra cita el día {entity.FechaInspeccion:yyyy-MM-dd}");
                return Result.Failure<Cita, DomainError>(CitaErrors.Database($"No se puede actualizar: El vehículo con matrícula {entity.Matricula} ya tiene otra cita asignada para ese día."));
            }
        }

        if (entity.Dni != existente.Dni || entity.FechaInspeccion.Date != existente.FechaInspeccion.Date) {
            if (ContarCitasPropietarioEnFecha(entity.Dni, entity.FechaInspeccion) >= AppConfig.MaxVehiculosPorDni) {
                _logger.Warning($"Validación fallida en Update: El propietario {entity.Dni} ya supera las {AppConfig.MaxVehiculosPorDni} citas.");
                return Result.Failure<Cita, DomainError>(CitaErrors.Database($"No se puede actualizar: El propietario con DNI {entity.Dni} ya tiene el límite de {AppConfig.MaxVehiculosPorDni} citas asignadas para ese día."));
            }
        }

        try {
            const string Sql = @"
                UPDATE Citas SET 
                    Matricula = @Matricula, Marca = @Marca, Modelo = @Modelo, 
                    Cilindrada = @Cilindrada, Motor = @Motor, Dni = @Dni, 
                    FechaMatriculacion = @FechaMatriculacion, FechaInspeccion = @FechaInspeccion, 
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            _connection.Execute(Sql, new {
                Id = id,
                entity.Matricula,
                entity.Marca,
                entity.Modelo,
                entity.Cilindrada,
                Motor = (int)entity.Motor,
                entity.Dni,
                FechaMatriculacion = entity.FechaMatriculacion.ToString("yyyy-MM-dd"),
                FechaInspeccion = entity.FechaInspeccion.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Result.Success<Cita, DomainError>(GetById(id)!);
        }
        catch (Exception ex) {
            _logger.Error(ex, $"Error al updatear la cita {id}");
            return Result.Failure<Cita, DomainError>(CitaErrors.Database(ex.Message));
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.Delete" />
    public Cita? Delete(int id, bool isLogical = true) {
        _logger.Debug($"Eliminando cita {id}. Borrado lógico: {isLogical}");
        var existente = GetById(id);
        if (existente == null) return null;

        try {
            if (isLogical) { // si es logico se actualiza el isDeleted
                const string Sql = "UPDATE Citas SET IsDeleted = 1, DeletedAt = @DeletedAt, UpdatedAt = @UpdatedAt WHERE Id = @Id";
                _connection.Execute(Sql, new { 
                    Id = id, 
                    DeletedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 
                    UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") 
                });
            } else { // si no, se elimina de la BD
                const string Sql = "DELETE FROM Citas WHERE Id = @Id";
                _connection.Execute(Sql, new { Id = id });
            }

            return existente;
        }
        catch (Exception ex) {
            _logger.Error(ex, $"Error al eliminar la cita {id}");
            return null;
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.DeleteAll" />
    public bool DeleteAll() {
        _logger.Warning("Eliminando todos los registros de Citas...");
        try {
            _connection.Execute("DELETE FROM Citas;");
            return true;
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al vaciar la tabla Citas");
            return false;
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.Restore" />
    public Result<Cita, DomainError> Restore(int id) {
        try {
            _logger.Debug($"Restaurado cita borrada con ID: {id}");
            var existente = GetById(id);
            if (existente == null) {
                return Result.Failure<Cita, DomainError>(CitaErrors.Database("No se puede restaurar una Cita que no existe."));
            }

            if (!existente.IsDeleted) {
                return Result.Success<Cita, DomainError>(existente);
            }

            // validacion de regla RN-05 al restaurar
            const string SqlCheckMatricula = "SELECT COUNT(1) FROM Citas WHERE Matricula = @Matricula AND date(FechaInspeccion) = date(@Fecha) AND IsDeleted = 0 AND Id <> @Id";
            var yaExisteActiva = _connection.ExecuteScalar<int>(SqlCheckMatricula, new { 
                existente.Matricula, 
                Fecha = existente.FechaInspeccion.ToString("yyyy-MM-dd"),
                Id = id
            }) > 0; // si es >0 es pq ya tiene otra cita oara ese dia

            if (yaExisteActiva) {
                return Result.Failure<Cita, DomainError>(CitaErrors.Database("No se puede restaurar: El vehículo ya cuenta con otra cita activa ese mismo día."));
            }
            
            // validacion de la regla RN-06 al restaurar
            const string SqlCheckDni = "SELECT COUNT(1) FROM Citas WHERE Dni = @Dni AND date(FechaInspeccion) = date(@Fecha) AND IsDeleted = 0 AND Id <> @Id";
            var citasDelPropietario = _connection.ExecuteScalar<int>(SqlCheckDni, new { 
                existente.Dni, 
                Fecha = existente.FechaInspeccion.ToString("yyyy-MM-dd"),
                Id = id
            });

            if (citasDelPropietario >= AppConfig.MaxVehiculosPorDni) {
                return Result.Failure<Cita, DomainError>(CitaErrors.Database($"No se puede actualizar: El propietario con DNI {existente.Dni} ya tiene el límite de {AppConfig.MaxVehiculosPorDni} citas asignadas para ese día."));
            }

            const string SqlUpdate = "UPDATE Citas SET IsDeleted = 0, DeletedAt = NULL, UpdatedAt = @UpdatedAt WHERE Id = @Id";
            _connection.Execute(SqlUpdate, new { Id = id, UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });

            return Result.Success<Cita, DomainError>(GetById(id)!);
        }
        catch (Exception ex) {
            _logger.Error(ex, $"Error al restaurr la cita {id}");
            return Result.Failure<Cita, DomainError>(CitaErrors.Database(ex.Message));
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.GetByMatricula" />
    public Cita? GetByMatricula(string matricula) {
        try {
            const string Sql = "SELECT * FROM Citas WHERE Matricula = @Matricula AND IsDeleted = 0";
            var entity = _connection.QueryFirstOrDefault<CitaEntity>(Sql, new { Matricula = matricula });
            return entity?.ToModel();
        }
        catch (Exception ex) {
            _logger.Error(ex, $"Error al buscar matricula {matricula} ");
            return null;
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.ExisteCitaParaVehiculoEnFecha" />
    public bool ExisteCitaParaVehiculoEnFecha(string matricula, DateTime fechaInspeccion) {
        const string Sql = "SELECT COUNT(1) FROM Citas WHERE Matricula = @Matricula AND date(FechaInspeccion) = date(@Fecha) AND IsDeleted = 0";
        return _connection.ExecuteScalar<int>(Sql, new { Matricula = matricula, Fecha = fechaInspeccion.ToString("yyyy-MM-dd") }) > 0; // si es mayor que 0 si que existe
    }
    
    /// <inheritdoc cref="ICitaRepository.ContarCitasPropietarioEnFecha" />
    public int ContarCitasPropietarioEnFecha(string dni, DateTime fechaInspeccion) {
        const string Sql = "SELECT COUNT(1) FROM Citas WHERE Dni = @Dni AND date(FechaInspeccion) = date(@Fecha) AND IsDeleted = 0";
        return _connection.ExecuteScalar<int>(Sql, new { Dni = dni, Fecha = fechaInspeccion.ToString("yyyy-MM-dd") });
    }
    
    /// <inheritdoc cref="ICitaRepository.CountCita" />
    public int CountCita(bool incluirEliminados = false) {
        try {
            var sql = incluirEliminados ? "SELECT COUNT(1) FROM Citas" : "SELECT COUNT(1) FROM Citas WHERE IsDeleted = 0";
            return _connection.ExecuteScalar<int>(sql);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al contar citas");
            return 0;
        }
    }

    /// <inheritdoc cref="ICitaRepository.GetWithFilters" />
    public Result<IEnumerable<Cita>, DomainError> GetWithFilters(DateTime fechaInicio, DateTime? fechaFin, int pagina, int tamPagina, string? searchText = null,
        string motorSeleccionado = "todos", bool incluirEliminados = false) {
        try {
            _logger.Debug("Ejecutando consulta con filtros mulktiparametros...");
            
            var motorValue = -1;
            if (!motorSeleccionado.Equals("todos", StringComparison.OrdinalIgnoreCase)) {
                if (string.Equals(motorSeleccionado, "gasolina", StringComparison.OrdinalIgnoreCase)) motorValue = 0;
                else if (string.Equals(motorSeleccionado, "diesel", StringComparison.OrdinalIgnoreCase)) motorValue = 1;
                else if (string.Equals(motorSeleccionado, "electrico", StringComparison.OrdinalIgnoreCase)) motorValue = 2;
                else if (string.Equals(motorSeleccionado, "hibrido", StringComparison.OrdinalIgnoreCase)) motorValue = 3;
            }

            const string Sql = @"
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
                LIMIT @Limit OFFSET @Offset";

            var entities = _connection.Query<CitaEntity>(Sql, new {
                FechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                FechaFin = fechaFin?.ToString("yyyy-MM-dd"),
                IncluirEliminados = incluirEliminados ? 1 : 0,
                MotorText = motorSeleccionado.ToLower(),
                MotorInt = motorValue,
                Search = string.IsNullOrWhiteSpace(searchText) ? null : $"%{searchText.ToLower()}%",
                Limit = tamPagina,
                Offset = (pagina - 1) * tamPagina
            });

            return Result.Success<IEnumerable<Cita>, DomainError>(entities.Select(e => e.ToModel()!));
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error en GetWithFilters");
            return Result.Failure<IEnumerable<Cita>, DomainError>(CitaErrors.Database(ex.Message));
        }
    }

    /// <inheritdoc cref="ICitaRepository.CountCitasFiltradas" />
    public int CountCitasFiltradas(string? searchText, DateTime fechaInicio, DateTime? fechaFin, bool incluirEliminados,
        string motorSeleccionado = "todos") {
        try {
            var motorValue = -1;
            if (!motorSeleccionado.Equals("todos", StringComparison.OrdinalIgnoreCase)) {
                if (string.Equals(motorSeleccionado, "gasolina", StringComparison.OrdinalIgnoreCase)) motorValue = 0;
                else if (string.Equals(motorSeleccionado, "diesel", StringComparison.OrdinalIgnoreCase)) motorValue = 1;
                else if (string.Equals(motorSeleccionado, "electrico", StringComparison.OrdinalIgnoreCase)) motorValue = 2;
                else if (string.Equals(motorSeleccionado, "hibrido", StringComparison.OrdinalIgnoreCase)) motorValue = 3;
            }

            const string Sql = @"
                SELECT COUNT(1) FROM Citas 
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

            return _connection.ExecuteScalar<int>(Sql, new {
                FechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                FechaFin = fechaFin?.ToString("yyyy-MM-dd"),
                IncluirEliminados = incluirEliminados ? 1 : 0,
                MotorText = motorSeleccionado.ToLower(),
                MotorInt = motorValue,
                Search = string.IsNullOrWhiteSpace(searchText) ? null : $"%{searchText.ToLower()}%"
            });
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al contar citas filtradas en Dapper");
            return 0;
        }
    }
    
    // funciones auxiliares
    private void EnsureTable(bool dropData) {
        if (_connection.State != ConnectionState.Open) _connection.Open(); // verificacion apertura conexion
        if (dropData) _connection.Execute("DROP TABLE IF EXISTS Citas");

        _connection.Execute(@"
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
            );");
    }

    private int CountTotal() => // n.º de citas
        _connection.ExecuteScalar<int>("SELECT COUNT(1) FROM Citas");
}