using Manager.Cache.Common;
using Serilog;

namespace Manager.Cache;

public class LruCache<TKey, TValue> : ICache<TKey, TValue> where TKey : notnull {
    
    private readonly int _capacidad;
    private readonly Dictionary<TKey, TValue> _data = new();
    private readonly LinkedList<TKey> _usageOrder = [];
    private readonly ILogger _logger = Log.ForContext<LruCache<TKey, TValue>>();
    
    // constructor que valida que no metan una capacidad negativa
    public LruCache(int capacity) {
        if (capacity <= 0) {
            throw new ArgumentException("La capacidad de la caché debe ser mayor que 0.", nameof(capacity));
        }
        _capacidad = capacity;
    }
    
    /// <inheritdoc />
    public void Add(TKey key, TValue value) {
        _logger.Debug("Intentando añadir clave: {Key}", key);

        // si la clave ya existe
        if (_data.TryGetValue(key, out var existingValue)) {
            _logger.Debug("Clave {Key} ya existe. Actualizando valor.", key);
            _data[key] = value; // update valor
            RefreshUsage(key); 
            return;
        }

        // si la caché está llena
        if (_data.Count >= _capacidad) {
            var oldestKey = _usageOrder.First!.Value;
            _logger.Information("Caché llena ({Capacidad}). Desalojando elemento menos usado: {OldestKey}", _capacidad, oldestKey);
            
            _usageOrder.RemoveFirst();
            _data.Remove(oldestKey);
        }

        // si no pasa nada, se ñañade a la caché
        _data.Add(key, value);
        _usageOrder.AddLast(key);
        _logger.Debug("Elemento añadido con éxito.");
    }
    
    /// <inheritdoc />
    public TValue? Get(TKey key) {
        _logger.Debug("Buscando clave: {Key}", key);

        // si no encuentra el objeto
        if (!_data.TryGetValue(key, out var value)) return default;

        _logger.Debug("Clave {Key} encontrada. Rejuveneciendo prioridad...", key);
        RefreshUsage(key);
        return value;
    }
    
    /// <inheritdoc />
    public bool Remove(TKey key) {
        _logger.Debug("Intentando eliminar clave: {Key}", key);

        // si el remove devuelve false es porque no ha podido borrar
        if (!_data.Remove(key)) return false;
        
        _usageOrder.Remove(key);
        _logger.Debug("Clave {Key} eliminada correctamente de las estructuras.", key);
        return true;
    }
    
    /// <inheritdoc />
    public void DisplayStatus() {
        _logger.Information("Capacidad: {DataCount}/{Capacidad}", _data.Count, _capacidad);
        _logger.Information("Historial de uso (Menos usado -> Más usado): {Order}", string.Join(" -> ", _usageOrder));
    }
    
    /// <summary> Mueve una clave a la última posición indicando que es la más recientemente usada </summary>
    private void RefreshUsage(TKey key) {
        _usageOrder.Remove(key);
        _usageOrder.AddLast(key);
    }
}