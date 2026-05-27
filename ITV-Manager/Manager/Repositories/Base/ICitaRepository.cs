using CSharpFunctionalExtensions;
using Manager.Errors.Common;
using Manager.Models;

namespace Manager.Repositories.Base;

public interface ICitaRepository : ICrudRepository<Cita, int> {
    
    /// <summary> Busca una cita activa por su matrícula </summary>
    Cita? GetByMatricula(string matricula); // nullable por si no encuentra
    
    /// <summary> RN-05. Verifica si un vehículo ya tiene una cita programada para un día específico </summary>
    /// <returns>True si ya tiene cita, false si no</returns>
    bool ExisteCitaParaVehiculoEnFecha(string matricula, DateTime fechaInspeccion);
    
    /// <summary> RN-06. Cuenta cuántas citas tiene registradas un mismo DNI para una fecha concreta </summary>
    /// <returns>N.º de citas para el dni en la fecha especificada</returns>
    int ContarCitasPropietarioEnFecha(string dni, DateTime fechaInspeccion);
    
    /// <summary> Obtiene el número total de citas registradas (para paginacion) </summary>
    int CountCita(bool incluirEliminados = false);
    
    /// <summary>
    /// Obtiene un listado paginado de citas aplicando un filtrado multiparámetro
    /// </summary>
    /// <param name="fechaInicio">Fecha inicial a partir de la cual se buscarán las citas</param>
    /// <param name="fechaFin">fecha final (opcional) que delimitará el tramo para buscar las citas</param>
    /// <param name="pagina">Num de la pagina que se desea recuperar</param>
    /// <param name="tamPagina">Cuantas citas habrá en la pagína de máximo</param>
    /// <param name="searchText">Texto libre opcional para filtrar por múltiples campos</param>
    /// <param name="motorSeleccionado">Tipo de motor</param>
    /// <param name="incluirEliminados">Mostrar o no las citas eliminadas</param>
    /// <returns></returns>
    Result<IEnumerable<Cita>, DomainError> GetWithFilters(
        DateTime fechaInicio, 
        DateTime? fechaFin, 
        int pagina, 
        int tamPagina, 
        string? searchText = null, 
        string motorSeleccionado = "todos", 
        bool incluirEliminados = false);
    
    /// <summary> Cuenta las citas que cumplen con los filtros de búsqueda aplicados </summary>
    int CountCitasFiltradas(
        string? searchText, 
        DateTime fechaInicio, 
        DateTime? fechaFin, 
        bool incluirEliminados, 
        string motorSeleccionado = "todos");
}