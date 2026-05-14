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
        public void ToModelFromDto_DeberiaSerSuccess() {
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
        public void ToDtoFromModel_DeberiaSerSuccess() {
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
        
        [Test]
        public void ToModelFromEntity_DeberiaSerSuccess() {
            var res = _citaEntity.ToModel();
            
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
        public void ToEntityFromModel_DeberiaSerSuccess() {
            var res = _cita.ToEntity();
            
            res.Should().NotBeNull();
            res.Id.Should().Be(1);
            res.Matricula.Should().Be("1111BBB");
            res.Dni.Should().Be("12345678Z");
            res.Marca.Should().Be("Skoda");
            res.Modelo.Should().Be("Octavia");
            res.Cilindrada.Should().Be(2000);
            res.Motor.Should().Be(1);
            res.FechaMatriculacion.Should().Be(new DateTime(2005, 10, 01));
            res.FechaInspeccion.Should().Be(new DateTime(2026, 06, 14));
            res.CreatedAt.Should().Be(new DateTime(2026, 05, 14));
            res.UpdatedAt.Should().Be(new DateTime(2026, 05, 14));
            res.IsDeleted.Should().Be(false);
            res.DeletedAt.Should().Be(null);
        }
        
        [Test]
        public void ToModelFromEntities_DeberiaSerSuccess() {
            var entities = new List<CitaEntity> { _citaEntity, _citaEntity, _citaEntity };

            var res = entities.ToModel();

            res.Should().HaveCount(3);
        }
        
        [Test]
        public void ToModel_CuandoDeletedAtTieneFecha_DeberiaSerSuccess() {
            // arrange
            var dtoConBorrado = _citaDto with { IsDeleted = true, DeletedAt = "2026-05-20T10:00:00" };

            // act
            var res = dtoConBorrado.ToModel();

            // assert
            res.DeletedAt.Should().Be(new DateTime(2026, 05, 20, 10, 00, 00));
        }
    }
    
    [TestFixture]
    public class CasosIncorrectos {
        
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
        public void ToModel_CuandoMotorEsInvalido_DeberiaUsarValoresPorDefecto() {
            
            // arrange
            var dtoInvalido = _citaDto with { Motor = "Invalido" };

            // act
            var res = dtoInvalido.ToModel();

            // assert
            res.Motor.Should().Be(Cita.TiposMotor.Gasolina);
        }
        
        [Test]
        public void ToModel_CuandoFechaTieneFormatoInvalido_DeberiaLanzarFormatException() {
            
            // arrange
            var dtoBasura = _citaDto with { FechaMatriculacion = "Invalida" };

            // act
            Action act = () => dtoBasura.ToModel(); // en la accion de pasar a model deberia fallar, por eso no se puede almacenar

            // assert
            act.Should().Throw<FormatException>();
        }
        
        [Test]
        public void ToModelFromEntity_CuandoEsNull_DeberiaRetornarNull() {
            
            // arrange
            CitaEntity? entityNula = null;

            // act
            var res = entityNula.ToModel();

            // assert
            res.Should().BeNull();
        }
        
        [Test]
        public void ToModelFromEntities_CuandoLaListaContieneNulos_DeberiaFiltrarlos() {
            
            // arrange
            var entities = new List<CitaEntity?> { _citaEntity, null, _citaEntity };

            // act
            var res = entities!.ToModel(); // ! para que ignore la posibilidad de null

            // assert
            res.Should().HaveCount(2);
        }
        
        [Test]
        public void ToDto_CuandoLaCitaEstaBorrada_DeberiaMapearFechaBorrado() {
            
            // arrange
            var citaBorrada = _cita with { 
                IsDeleted = true, 
                DeletedAt = new DateTime(2026, 05, 20, 00, 00, 00) 
            };

            // act
            var res = citaBorrada.ToDto();

            // assert
            res.IsDeleted.Should().BeTrue();
            res.DeletedAt.Should().NotBeEmpty();
            res.DeletedAt.Should().Contain("2026-05-20T00:00:00");
        }
    }
}