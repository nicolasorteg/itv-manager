using Manager.Models;
using Manager.Storage.Common;

namespace Manager.Storage.Json;

/// <summary> Contrato para guardado y escritura de citas en formato JSON </summary>
public interface ICitaJsonStorage : IStorage<Cita> { }