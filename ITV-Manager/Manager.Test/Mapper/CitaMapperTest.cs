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
                FechaMatriculacion = DateTime.Now.AddYears(-20),
                FechaInspeccion = DateTime.Now.AddDays(10),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
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
                FechaInspeccion = "2026-06-12",
                CreatedAt = "2026-05-14",
                UpdatedAt = "2026-05-14",
                IsDeleted = false,
                DeletedAt = "null"
            };   
            
            _citaEntity = new CitaEntity {
                Id = 1,
                Matricula = "1111BBB",
                Dni = "12345678Z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = 2000,
                Motor = 1,
                FechaMatriculacion = DateTime.Now.AddYears(-20),
                FechaInspeccion = DateTime.Now.AddDays(10),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                DeletedAt = null
            };  
        }
        
        
        [Test]
        public void ToModel_DeberiaSerSucces() {
            var res = _citaDto.ToModel();
            
            res.Should().NotBeNull();
            res.Id.Should().Be(1);
            res.Matricula.Should().Be("1111BBB");
            res.Dni.Should().Be("12345678Z");
            res.Marca.Should().Be("Skoda");
            res.Modelo.Should().Be("Octavia");
            res.Cilindrada.Should().Be(2000);
            res.Motor.Should().Be(Cita.TiposMotor.Diesel);
            res.FechaMatriculacion.Should().Be(DateTime.Now.AddYears(-20));
            res.FechaInspeccion.Should().Be(DateTime.Now.AddDays(10));
            res.CreatedAt.Should().Be(DateTime.UtcNow);
            res.UpdatedAt.Should().Be(DateTime.UtcNow);
            res.IsDeleted.Should().Be(false);
            res.DeletedAt.Should().Be(null);
        }
        
        
        
    }
    
    [TestFixture]
    public class CasosIncorrectos {
        
    }
}