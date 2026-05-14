using System.Globalization;
using Manager.Dto;
using Manager.Entity;
using Manager.Models;

namespace Manager.Mapper;

public static class CitaMapper {
    
    // formato fecha sin hora
    private const string DateFormat = "d";
    // formato fecha con hora
    private const string DateTimeFormat = "s";
    
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    /// <summary> Conversión DTO -> Model </summary>
    public static Cita ToModel(this CitaDto dto) {
        return new Cita {
            Id = dto.Id,
            Matricula = dto.Matricula,
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            Cilindrada = dto.Cilindrada,
            Motor = Enum.TryParse(dto.Motor, out Cita.TiposMotor tipo) ? tipo : Cita.TiposMotor.Gasolina, // si falla default gasolina
            Dni = dto.Dni,
            FechaMatriculacion = DateTime.Parse(dto.FechaMatriculacion, InvariantCulture),
            FechaInspeccion = DateTime.Parse(dto.FechaInspeccion, InvariantCulture),
            CreatedAt = DateTime.Parse(dto.CreatedAt, InvariantCulture),
            UpdatedAt = DateTime.Parse(dto.UpdatedAt, InvariantCulture),
            IsDeleted = dto.IsDeleted,
            DeletedAt = string.IsNullOrEmpty(dto.DeletedAt) 
                ? null 
                : DateTime.Parse(dto.DeletedAt, InvariantCulture)
        };
    }
    
    /// <summary> Conversión Model -> DTO </summary>
    public static CitaDto ToDto(this Cita cita) {
        return new CitaDto(
            cita.Id,
            cita.Matricula,
            cita.Dni,
            cita.Marca,
            cita.Modelo,
            cita.Cilindrada,
            cita.Motor.ToString(),
            cita.FechaMatriculacion.ToString(DateFormat, InvariantCulture), // sin hora
            cita.FechaInspeccion.ToString(DateFormat, InvariantCulture), // sin hora
            cita.CreatedAt.ToString(DateTimeFormat, InvariantCulture), // metadatos con hora
            cita.UpdatedAt.ToString(DateTimeFormat, InvariantCulture),
            cita.IsDeleted,
            cita.DeletedAt?.ToString(DateTimeFormat, InvariantCulture) ?? string.Empty // si no está borrado vacio
        );
    }
    
    /// <summary> Conversión Entity -> Model </summary>
    public static Cita? ToModel(this CitaEntity? entity) {
        
        // si la querie no devuelve nada
        if (entity == null) return null;

        return new Cita {
            Id = entity.Id,
            Matricula = entity.Matricula,
            Marca = entity.Marca,
            Modelo = entity.Modelo,
            Cilindrada = entity.Cilindrada,
            Motor = (Cita.TiposMotor)entity.Motor, // tipos motor en el entity es un int (0-3)
            Dni = entity.Dni,
            FechaMatriculacion = entity.FechaMatriculacion,
            FechaInspeccion = entity.FechaInspeccion,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsDeleted = entity.IsDeleted,
            DeletedAt = entity.DeletedAt
        };
    }
    
    /// <summary> Conversión Model -> Entity </summary>
    public static CitaEntity ToEntity(this Cita model) {
        return new CitaEntity {
            Id = model.Id,
            Matricula = model.Matricula,
            Marca = model.Marca,
            Modelo = model.Modelo,
            Cilindrada = model.Cilindrada,
            Motor = (int)model.Motor, // se guarda el "indice"
            Dni = model.Dni,
            FechaMatriculacion = model.FechaMatriculacion,
            FechaInspeccion = model.FechaInspeccion,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted,
            DeletedAt = model.DeletedAt
        };
    }
    
    /// <summary> Conversión Listado de Entity -> Listado de Model </summary>
    public static IEnumerable<Cita> ToModel(this IEnumerable<CitaEntity> entities) { 
        return entities
            .Select(e => e.ToModel())
            .OfType<Cita>(); // garantiza el tipo
    }
}