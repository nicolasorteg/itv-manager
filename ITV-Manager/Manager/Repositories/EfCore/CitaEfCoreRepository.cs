using CSharpFunctionalExtensions;
using Manager.Entity;
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
        throw new NotImplementedException();
    }
    
    /// <inheritdoc cref="ICitaRepository.Update" />
    public Result<Cita, DomainError> Update(int id, Cita entity) {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc cref="ICitaRepository.Delete" />
    public Cita? Delete(int id, bool isLogical = true) {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc cref="ICitaRepository.DeleteAll" />
    public bool DeleteAll() {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc cref="ICitaRepository.Restore" />
    public Result<Cita, DomainError> Restore(int id) {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc cref="ICitaRepository.GetByMatricula" />
    public Cita? GetByMatricula(string matricula) {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc cref="ICitaRepository.ExisteCitaParaVehiculoEnFecha" />
    public bool ExisteCitaParaVehiculoEnFecha(string matricula, DateTime fechaInspeccion) {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc cref="ICitaRepository.ContarCitasPropietarioEnFecha" />
    public int ContarCitasPropietarioEnFecha(string dni, DateTime fechaInspeccion) {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc cref="ICitaRepository.CountCita" />
    public int CountCita(bool incluirEliminados = false) {
        throw new NotImplementedException();
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