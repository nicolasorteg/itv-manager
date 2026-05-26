using CSharpFunctionalExtensions;
using Manager.Cache.Common;
using Manager.Config;
using Manager.Errors.Citas;
using Manager.Errors.Common;
using Manager.Models;
using Manager.Repositories.Base;
using Manager.Validators.Common;
using Serilog;

namespace Manager.Service.Manager;

public class ManagerService: IManagerService {
    
    private readonly ICitaRepository _citaRepository;
    private readonly IValidator<Cita> _validator;
    private readonly ICache<int, Cita> _cache;

    public ManagerService(ICitaRepository citaRepository, IValidator<Cita> validator, ICache<int, Cita> cache) {
        _citaRepository = citaRepository;
        _validator = validator;
        _cache = cache;
    }

    /// <inheritdoc cref="IManagerService.ObtenerPorId" />
    public Result<Cita, DomainError> ObtenerPorId(int id) {
        Log.Debug($"Buscando la Cita de ID -> {id}");
        
        // si esta en cache se devuelve directamente, si no se pasa al repo
        var citaCache = _cache.Get(id);
        if (citaCache != null) return Result.Success<Cita, DomainError>(citaCache);

        var citaRepo = _citaRepository.GetById(id);
        if (citaRepo == null) { // si tampoco está aqui es que no existe
            return Result.Failure<Cita, DomainError>(new CitaError.NotFound($"{id}"));
        }

        // añade a la cache para prioridad
        _cache.Add(id, citaRepo);
        return Result.Success<Cita, DomainError>(citaRepo);
    }
    
    /// <inheritdoc cref="IManagerService.ObtenerPorMatricula" />
    public Result<Cita, DomainError> ObtenerPorMatricula(string matricula) {
        var cita = _citaRepository.GetByMatricula(matricula);
        return cita != null 
            ? Result.Success<Cita, DomainError>(cita)
            : Result.Failure<Cita, DomainError>(new CitaError.NotFound($"{matricula}"));
    }
    
    /// <inheritdoc cref="IManagerService.ObtenerConFiltros" />
    public Result<IEnumerable<Cita>, DomainError> ObtenerConFiltros(DateTime fechaInicio, DateTime? fechaFin, int pagina, int tamPagina, string? searchText = null,
        string motorSeleccionado = "todos", bool incluirEliminados = false) =>
        _citaRepository.GetWithFilters(fechaInicio, fechaFin, pagina, tamPagina, searchText, motorSeleccionado, incluirEliminados);
    
    /// <inheritdoc cref="IManagerService.CrearCita" />
    public Result<Cita, DomainError> CrearCita(Cita cita) {
        Log.Debug($"Procesando creación de cita para matrícula-> {cita.Matricula}");

        // validacion campos cita
        return _validator.Validar(cita)
            .Ensure(c => !_citaRepository.ExisteCitaParaVehiculoEnFecha(c.Matricula, c.FechaInspeccion), // verificar RN-05
                new CitaError.InspeccionRepetida(cita.Matricula, cita.FechaInspeccion))
            .Ensure(c => _citaRepository.ContarCitasPropietarioEnFecha(c.Dni, c.FechaInspeccion) < AppConfig.MaxVehiculosPorDni, // verificar RN-06
                new CitaError.MaximosVehiculosAlcanzados(cita.Dni, cita.FechaInspeccion))
            .Bind(c => _citaRepository.Create(c))
            .Tap(c => _cache.Add(c.Id, c));// update cache
    }
    
    /// <inheritdoc cref="IManagerService.ActualizarCita" />
    public Result<Cita, DomainError> ActualizarCita(int id, Cita cita) {
        Log.Debug($"Procesando actualización de cita ID -> {id}");

        return _validator.Validar(cita) // validacion campos cita
            .Ensure(_ => _citaRepository.GetById(id) != null, // cita existente
                new CitaError.NotFound($"{id}"))
            .Bind(c => _citaRepository.Update(id, c))
            .Tap(c => _cache.Add(c.Id, c)); // update cache
    }
    
    /// <inheritdoc cref="IManagerService.EliminarCita" />
    public Result<Cita, DomainError> EliminarCita(int id, bool esLogico = true) {
        Log.Warning($"Eliminando cita ID: {id} (Borrado lógico: {esLogico})");
        
        var citaEliminada = _citaRepository.Delete(id, esLogico);
        if (citaEliminada == null) return Result.Failure<Cita, DomainError>(new CitaError.NotFound($"{id}"));
        
        // si se ha eliminado remove de la cache
        _cache.Remove(id);
        return Result.Success<Cita, DomainError>(citaEliminada);
    }
    
    /// <inheritdoc cref="IManagerService.RestaurarCita" />
    public Result<Cita, DomainError> RestaurarCita(int id) {
        Log.Debug($"Restaurando cita lógicamente borrada ID -> {id}");
        return _citaRepository.Restore(id).Tap(c => _cache.Add(c.Id, c));
    }
    
    /// <inheritdoc cref="IManagerService.EliminarTodasLasCitas" />
    public bool EliminarTodasLasCitas() {
        Log.Error("Eliminando de forma absoluta todo el historial de citas");
        var isEliminado = _citaRepository.DeleteAll();
        if (isEliminado) _cache.Clear();
        return isEliminado;
    }

    /// <inheritdoc cref="IManagerService.ContarCitasFiltradas" />
    public int ContarCitasFiltradas(string? searchText, DateTime fechaInicio, DateTime? fechaFin, bool incluirEliminados,
        string motorSeleccionado = "todos") =>
        _citaRepository.CountCitasFiltradas(searchText, fechaInicio, fechaFin, incluirEliminados, motorSeleccionado);
    

    /// <inheritdoc cref="IManagerService.ContarTotalCitas" />
    public int ContarTotalCitas(bool incluirEliminados = false) =>
        _citaRepository.CountCita(incluirEliminados);
    
    /// <inheritdoc cref="IManagerService.ObtenerTodas" />
    public IEnumerable<Cita> ObtenerTodas(int pagina = 1, int tamPagina = 10, bool incluirEliminados = false) =>
        _citaRepository.GetAll(pagina, tamPagina, incluirEliminados);

    public Result<Cita, DomainError> ImportarCita(Cita cita) {
        throw new NotImplementedException();
    }
}