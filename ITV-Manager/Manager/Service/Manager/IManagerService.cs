using CSharpFunctionalExtensions;
using Manager.Errors.Common;
using Manager.Models;

namespace Manager.Service.Manager;

public interface IManagerService {
    
    /// <summary> Obtiene una cita en base a un ID </summary>
    Result<Cita, DomainError> ObtenerPorId(int id);
    
    /// <summary> Obtiene una cita en base a una matricula pasada </summary>
    Result<Cita, DomainError> ObtenerPorMatricula(string matricula);
    
    /// <summary> Busca citas en base a varios filtros </summary>
    Result<IEnumerable<Cita>, DomainError> ObtenerConFiltros(
        DateTime fechaInicio, 
        DateTime? fechaFin, 
        int pagina, 
        int tamPagina, 
        string? searchText = null, 
        string motorSeleccionado = "todos", 
        bool incluirEliminados = false);
    
    /// <summary> Guarda una cita pasada </summary>
    Result<Cita, DomainError> CrearCita(Cita cita);
    
    /// <summary> Actualiza una cita pasada </summary>
    Result<Cita, DomainError> ActualizarCita(int id, Cita cita);
    
    /// <summary> Elimina una cita pasada </summary>
    Result<Cita, DomainError> EliminarCita(int id, bool esLogico = true);
    
    /// <summary> Restaura una cita pasada </summary>
    Result<Cita, DomainError> RestaurarCita(int id);
    
    /// <summary> Elimina todas las citas </summary>
    bool EliminarTodasLasCitas();
    
    /// <summary> Busca citas en base a varios filtros y devuelve el n.º encontrado </summary>
    int ContarCitasFiltradas(
        string? searchText, 
        DateTime fechaInicio, 
        DateTime? fechaFin, 
        bool incluirEliminados, 
        string motorSeleccionado = "todos");
    
    /// <summary> Cuenta la cantidad de citas existentes </summary>
    int ContarTotalCitas(bool incluirEliminados = false);

    IEnumerable<Cita> ObtenerTodas(int pagina, int tamPagina, bool incluirEliminados = false);
    
    Result<Cita, DomainError> ImportarCita(Cita cita);
}