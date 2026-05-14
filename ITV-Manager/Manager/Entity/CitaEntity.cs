using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manager.Entity;

/// <summary>
/// Representación de Cita para la BD
/// </summary>
[Table("Citas")]
public class CitaEntity {
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // autoincrement
    public int Id { get; set; }
    
    [Required]
    public string Matricula { get; set; } = string.Empty;
    
    [Required]
    public string Dni { get; set; } = string.Empty;

    [Required]
    [MaxLength(15)] // coincide con el validator
    public string Marca { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(15)]
    public string Modelo { get; set; } = string.Empty;
    
    [Required]
    public int Cilindrada { get; set; }
    
    [Required]
    public int Motor { get; set; }
    
    [Required]
    public DateTime FechaItv { get; set; }

    [Required]
    public DateTime FechaInspeccion { get; set; }

    [Column(TypeName = "datetime2")] public DateTime CreatedAt { get; set; } = DateTime.Now;
    [Column(TypeName = "datetime2")] public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = false;
    [Column(TypeName = "datetime2")] public DateTime? DeletedAt { get; set; } // nullable
}