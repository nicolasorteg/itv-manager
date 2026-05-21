using CSharpFunctionalExtensions;
using Manager.Config;
using Manager.Entity;
using Manager.Errors.Citas;
using Manager.Errors.Common;
using Manager.Factories;
using Manager.Mapper;
using Manager.Models;
using Manager.Repositories.Base;
using Serilog;

namespace Manager.Repositories.EfCore;

/// <summary> Repositorio que usa EFCore con SQLite para la BD </summary>
public class CitaEfCoreRepository: ICitaRepository {
    
    private readonly AppDbContext _context;
    private readonly ILogger _logger = Log.ForContext<CitaEfCoreRepository>();

    // constructor
    public CitaEfCoreRepository(AppDbContext context, bool dropData = false, bool seedData = false) {
        _context = context;

        if (dropData) {
            _logger.Warning("DropData activo: Eliminando base de datos...");
            _context.Database.EnsureDeleted();
        }

        _context.Database.EnsureCreated();

        if (!seedData || _context.Citas.Any()) return;
        _logger.Debug("Sembrando datos iniciales de citas en EFCore...");
        foreach (var cita in CitaFactory.Seed()) Create(cita);
    }
    
    /// <inheritdoc cref="ICitaRepository.GetById" />
    public Cita? GetById(int id) {
        try {
            _logger.Debug($"Obteniendo cita por ID: {id}");
            var entity = _context.Citas.FirstOrDefault(c => c.Id == id);
            return entity?.ToModel();
        }
        catch (Exception ex) {
            _logger.Error(ex, $"Error al obtener cita por ID {id}");
            return null;
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.Create" />
    public Result<Cita, DomainError> Create(Cita entity) {
        try {
            _logger.Debug($"Insertando nueva cita por EF para vehículo: {entity.Matricula}");

            // comprobaciones RN
            if (ExisteCitaParaVehiculoEnFecha(entity.Matricula, entity.FechaInspeccion)) {
                _logger.Warning($"Validación fallida: El vehículo {entity.Matricula} ya tiene una cita el día {entity.FechaInspeccion:yyyy-MM-dd}");
                return Result.Failure<Cita, DomainError>(CitaErrors.Database($"El vehículo con matrícula {entity.Matricula} ya tiene una cita asignada para ese día."));
            }
            if (ContarCitasPropietarioEnFecha(entity.Dni, entity.FechaInspeccion) >= AppConfig.MaxVehiculosPorDni) {
                _logger.Warning($"Validación fallida: El propietario con DNI {entity.Dni} ya supera el límite de {AppConfig.MaxVehiculosPorDni} citas");
                return Result.Failure<Cita, DomainError>(CitaErrors.Database($"El propietario con DNI {entity.Dni} no puede registrar más de {AppConfig.MaxVehiculosPorDni} citas el mismo día."));
            }

            // conversion
            var dbEntity = entity.ToEntity();
            dbEntity.CreatedAt = DateTime.Now;
            dbEntity.UpdatedAt = DateTime.Now;
            dbEntity.IsDeleted = false;

            _context.Citas.Add(dbEntity);
            _context.SaveChanges(); // gernera ID autoincremental

            _logger.Information($"✅ Cita guardada por EF correctamente con ID: {dbEntity.Id}");
            return Result.Success<Cita, DomainError>(dbEntity.ToModel()!);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al crear cita en EF Core");
            return Result.Failure<Cita, DomainError>(CitaErrors.Database(ex.Message));
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.Update" />
    public Result<Cita, DomainError> Update(int id, Cita entity) {
        
        _logger.Debug($"Actualizando cita por EF con ID: {id}");

        // busqueda entity
        var existente = _context.Citas.FirstOrDefault(c => c.Id == id);
        if (existente == null) {
            return Result.Failure<Cita, DomainError>(CitaErrors.NotFound($"No existe la cita con ID {id} para actualizar."));
        }

        // verificacion RN
        if (entity.Matricula != existente.Matricula || entity.FechaInspeccion.Date != existente.FechaInspeccion.Date) {
            if (ExisteCitaParaVehiculoEnFecha(entity.Matricula, entity.FechaInspeccion)) {
                _logger.Warning($"Validación fallida en Update: El vehículo {entity.Matricula} ya tiene otra cita el día {entity.FechaInspeccion:yyyy-MM-dd}");
                return Result.Failure<Cita, DomainError>(CitaErrors.Database($"No se puede actualizar: El vehículo con matrícula {entity.Matricula} ya tiene otra cita asignada para ese día."));
            }
        }
        if (entity.Dni != existente.Dni || entity.FechaInspeccion.Date != existente.FechaInspeccion.Date) {
            if (ContarCitasPropietarioEnFecha(entity.Dni, entity.FechaInspeccion) >= 3) {
                _logger.Warning($"Validación fallida en Update: El propietario {entity.Dni} ya supera las 3 citas.");
                return Result.Failure<Cita, DomainError>(CitaErrors.Database($"No se puede actualizar: El propietario con DNI {entity.Dni} ya tiene el límite de 3 citas asignadas para ese día."));
            }
        }

        try {
            existente.Matricula = entity.Matricula;
            existente.Marca = entity.Marca;
            existente.Modelo = entity.Modelo;
            existente.Cilindrada = entity.Cilindrada;
            existente.Motor = (int)entity.Motor;
            existente.Dni = entity.Dni;
            existente.FechaMatriculacion = entity.FechaMatriculacion;
            existente.FechaInspeccion = entity.FechaInspeccion;
            existente.UpdatedAt = DateTime.Now; 

            _context.SaveChanges();
            return Result.Success<Cita, DomainError>(existente.ToModel()!);
        }
        catch (Exception ex) {
            _logger.Error(ex, $"Error al actualizar la cita {id} en EF");
            return Result.Failure<Cita, DomainError>(CitaErrors.Database(ex.Message));
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.Delete" />
    public Cita? Delete(int id, bool isLogical = true) {
        
        _logger.Debug($"Eliminando cita {id} vía EF. Borrado lógico: {isLogical}");
        var entity = _context.Citas.FirstOrDefault(c => c.Id == id);
        if (entity == null) return null;
        
        try {
            var modelAntesDeBorrar = entity.ToModel();

            if (isLogical) {
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.Now;
                entity.UpdatedAt = DateTime.Now;
            } else {
                _context.Citas.Remove(entity);
            }

            _context.SaveChanges();
            return modelAntesDeBorrar;
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
            _context.Citas.RemoveRange(_context.Citas);
            _context.SaveChanges();
            return true;
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al vaciar la tabla Citas");
            return false;
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.Restore" />
    public Result<Cita, DomainError> Restore(int id) {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc cref="ICitaRepository.GetByMatricula" />
    public Cita? GetByMatricula(string matricula) {
        try {
            var entity = _context.Citas.FirstOrDefault(c => c.Matricula == matricula && !c.IsDeleted); // si esta marcada como borrada no la encontrará
            return entity?.ToModel();
        }
        catch (Exception ex) {
            _logger.Error(ex, $"Error al buscar matrícula {matricula}");
            return null;
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.ExisteCitaParaVehiculoEnFecha" />
    public bool ExisteCitaParaVehiculoEnFecha(string matricula, DateTime fechaInspeccion) =>
        _context.Citas.Any(c => c.Matricula == matricula && c.FechaInspeccion.Date == fechaInspeccion.Date && !c.IsDeleted);
    
    /// <inheritdoc cref="ICitaRepository.ContarCitasPropietarioEnFecha" />
    public int ContarCitasPropietarioEnFecha(string dni, DateTime fechaInspeccion) =>
        _context.Citas.Count(c => c.Dni == dni && c.FechaInspeccion.Date == fechaInspeccion.Date && !c.IsDeleted);
    
    /// <inheritdoc cref="ICitaRepository.CountCita" />
    public int CountCita(bool incluirEliminados = false) {
        try {
            return incluirEliminados ? _context.Citas.Count() : _context.Citas.Count(c => !c.IsDeleted);
        }
        catch (Exception ex) {
            _logger.Error(ex, "Error al contar citas");
            return 0;
        }
    }
    
    /// <inheritdoc cref="ICitaRepository.GetWithFilters" />
    public Result<IEnumerable<Cita>, DomainError> GetWithFilters(DateTime fechaInicio, DateTime? fechaFin, int pagina, int tamPagina, string? searchText = null,
        string motorSeleccionado = "todos", bool incluirEliminados = false) {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc cref="ICitaRepository.CountCitasFiltradas" />
    public int CountCitasFiltradas(string? searchText, DateTime fechaInicio, DateTime? fechaFin, bool incluirEliminados,
        string motorSeleccionado = "todos") {
        throw new NotImplementedException();
    }
}