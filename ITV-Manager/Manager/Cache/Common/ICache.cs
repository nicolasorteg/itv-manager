namespace Manager.Cache.Common;

/// <summary> Contrato para una caché genérica </summary>
/// <typeparam name="TKey">Tipo de la clave</typeparam>
/// <typeparam name="TValue">Tipo del objeto a guardar</typeparam>
public interface ICache<in TKey, TValue> where TKey : notnull {
    
    /// <summary>  Agrega un elemento a la caché. Si está llena, elimina el menos usado. </summary>
    void Add(TKey key, TValue value);
    
    /// <summary> Obtiene un elemento de la caché </summary>
    TValue? Get(TKey key);
    
    /// <summary> Elimina un elemento de la caché </summary>
    bool Remove(TKey key);
    
    /// <summary> Muestra el estado de la caché </summary>
    void DisplayStatus();

    /// <summary> Vacia la caché </summary>
    void Clear();
}