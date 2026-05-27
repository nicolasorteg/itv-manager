using CSharpFunctionalExtensions;
using Manager.Errors.Common;

namespace Manager.Repositories.Base;

/// <summary> Contrato genérico base para operaciones CRUD </summary>
/// <typeparam name="TEntity">Tipo del Modelo de Dominio</typeparam>
/// <typeparam name="TKey">Tipo del Id</typeparam>
public interface ICrudRepository<TEntity, in TKey> where TEntity : class {
    
    /// <summary> Obtiene una entidad por su ID </summary>
    TEntity? GetById(TKey id);

    /// <summary> Obtiene todas las citas de forma paginada </summary>
    IEnumerable<TEntity> GetAll(int pagina = 1, int tamPagina = 10, bool incluirEliminados = false);

    /// <summary> Crea una nueva entidad en el sistema </summary>
    Result<TEntity, DomainError> Create(TEntity entity);

    /// <summary> Actualiza una entidad existente </summary>
    Result<TEntity, DomainError> Update(TKey id, TEntity entity);

    /// <summary> Elimina una entidad (default borrado lógico). </summary>
    TEntity? Delete(TKey id, bool isLogical = true);

    /// <summary> Elimina todos los registros del sistema </summary>
    bool DeleteAll();

    /// <summary> Restaura una entidad eliminada lógicamente </summary>
    Result<TEntity, DomainError> Restore(TKey id);
}