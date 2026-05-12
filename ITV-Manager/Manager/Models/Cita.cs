namespace Manager.Models;

/// <summary> Modelo de Cita inmutable que centraliza datos del vehículo y de la inspección </summary>
public record Cita {
    
    public int Id { get; init; }
    
    public int Matricula { get; init; }
    public string Dni { get; init; } = string.Empty;
    public string Marca { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public int Cilindrada { get; init; }
    public TiposMotor Motor { get; init; }
    public DateTime FechaMatriculacion { get; init; }
    public DateTime FechaInspeccion { get; init; }
    
    // metadatos
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime UpdatedAt { get; init; } = DateTime.Now;
    public bool IsDeleted { get; init; } = false;
    public DateTime? DeletedAt { get; init; } = null;
    
    public enum TiposMotor { Gasolina, Diesel, Electrico, Hibrido }
}