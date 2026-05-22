using CSharpFunctionalExtensions;
using Manager.Errors.Common;
using Manager.Models;
using Manager.Repositories.Base;

namespace Manager.Repositories.Dapper;

/// <summary> Repositorio que usa Dapper para la BD </summary>
public class CitaDapperRepository: ICitaRepository {
    
    
    /// <inheritdoc cref="ICitaRepository.GetById" />
    public Cita? GetById(int id) {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc cref="ICitaRepository.GetAll" />
    public IEnumerable<Cita> GetAll(int pagina = 1, int tamPagina = 10, bool incluirEliminados = false) {
        throw new NotImplementedException();
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