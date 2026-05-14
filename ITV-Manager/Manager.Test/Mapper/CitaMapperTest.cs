using FluentAssertions;
using Manager.Dto;
using Manager.Entity;
using Manager.Mapper;
using Manager.Models;

namespace Manager.Test.Mapper;

/// <summary>
/// Clase que almacena los test del mapper.
/// </summary>
[TestFixture]
public class CitaMapperTest {


    [TestFixture]
    public class CasosCorrectos {
        
        private Cita _cita = null!;
        private CitaDto _citaDto = null!;
        private CitaEntity _citaEntity = null!;

        [SetUp]
        public void SetUp() {
            
            _cita = new Cita {
                Id = 1,
                Matricula = "1111BBB",
                Dni = "12345678Z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = 2000,
                Motor = Cita.TiposMotor.Diesel,
                FechaMatriculacion = new DateTime(2005, 10, 01),
                FechaInspeccion = new DateTime(2026, 06, 14),
                CreatedAt = new DateTime(2026, 05, 14, 00, 00, 00),
                UpdatedAt = new DateTime(2026, 05, 14, 00, 00, 00),
                IsDeleted = false,
                DeletedAt = null
            };
            
            _citaDto = new CitaDto {
                Id = 1,
                Matricula = "1111BBB",
                Dni = "12345678Z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = 2000,
                Motor = "Diesel",
                FechaMatriculacion = "2005-10-01",
                FechaInspeccion = "2026-06-14",
                CreatedAt = "2026-05-14T00:00:00",
                UpdatedAt = "2026-05-14T00:00:00",
                IsDeleted = false,
                DeletedAt = ""
            };   
            
            _citaEntity = new CitaEntity {
                Id = 1,
                Matricula = "1111BBB",
                Dni = "12345678Z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = 2000,
                Motor = 1,
                FechaMatriculacion = new DateTime(2005, 10, 01),
                FechaInspeccion = new DateTime(2026, 06, 14),
                CreatedAt = new DateTime(2026, 05, 14),
                UpdatedAt = new DateTime(2026, 05, 14),
                IsDeleted = false,
                DeletedAt = null
            };  
        }
        
        
        [Test]
        public void ToModel_DeberiaSerSuccess() {
            var res = _citaDto.ToModel();
            
            res.Should().NotBeNull();
            res.Id.Should().Be(1);
            res.Matricula.Should().Be("1111BBB");
            res.Dni.Should().Be("12345678Z");
            res.Marca.Should().Be("Skoda");
            res.Modelo.Should().Be("Octavia");
            res.Cilindrada.Should().Be(2000);
            res.Motor.Should().Be(Cita.TiposMotor.Diesel);
            res.FechaMatriculacion.Should().Be(new DateTime(2005, 10, 01));
            res.FechaInspeccion.Should().Be(new DateTime(2026, 06, 14));
            res.CreatedAt.Should().Be(new DateTime(2026, 05, 14));
            res.UpdatedAt.Should().Be(new DateTime(2026, 05, 14));
            res.IsDeleted.Should().Be(false);
            res.DeletedAt.Should().Be(null);
        }
        
        [Test]
        public void ToDto_DeberiaSerSuccess() {
            var res = _cita.ToDto();
            
            res.Should().NotBeNull();
            res.Id.Should().Be(1);
            res.Matricula.Should().Be("1111BBB");
            res.Dni.Should().Be("12345678Z");
            res.Marca.Should().Be("Skoda");
            res.Modelo.Should().Be("Octavia");
            res.Cilindrada.Should().Be(2000);
            res.Motor.Should().Be("Diesel");
            res.FechaMatriculacion.Should().Be("2005-10-01");
            res.FechaInspeccion.Should().Be("2026-06-14");
            res.CreatedAt.Should().Be("2026-05-14T00:00:00");
            res.UpdatedAt.Should().Be("2026-05-14T00:00:00");
            res.IsDeleted.Should().Be(false);
            res.DeletedAt.Should().Be("");
        }
        
        
        
    }
    
    [TestFixture]
    public class CasosIncorrectos {
        
    }
}