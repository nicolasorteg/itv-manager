using Manager.Models;
using Manager.Storage.Common;

namespace Manager.Storage.Csv;

/// <summary> Contrato para guardado y escritura de citas en formato CSV </summary>
public interface ICitaCsvStorage : IStorage<Cita> {}