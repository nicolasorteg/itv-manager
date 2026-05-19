using System.Xml.Serialization;

namespace Manager.Dto;

/// <summary>
/// Record para transferencia de datos
/// </summary>
[XmlRoot("Cita")]
public record CitaDto(
    [property: XmlElement("Id")] int Id,
    [property: XmlElement("Matricula")] string Matricula,
    [property: XmlElement("Dni")] string Dni,
    [property: XmlElement("Marca")] string Marca,
    [property: XmlElement("Modelo")] string Modelo,
    [property: XmlElement("Cilindrada")] int Cilindrada,
    [property: XmlElement("Motor")] string Motor,
    [property: XmlElement("FechaMatriculacion")] string FechaMatriculacion,
    [property: XmlElement("FechaInspeccion")] string FechaInspeccion,
    [property: XmlElement("CreatedAt")] string CreatedAt,
    [property: XmlElement("UpdatedAt")] string UpdatedAt,
    [property: XmlElement("IsDeleted")] bool IsDeleted,
    [property: XmlElement("DeletedAt")] string? DeletedAt

) {
    // constructor vacio para serializadores
    public CitaDto() : this(0, "", "", "", "", 0, "", "", "", "", "", false, "") { }
}