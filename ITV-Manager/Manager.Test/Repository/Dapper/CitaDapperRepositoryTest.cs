using System.Data;
using FluentAssertions;
using Manager.Config;
using Manager.Errors.Citas;
using Manager.Models;
using Manager.Repositories.Dapper;
using Microsoft.Data.Sqlite;

namespace Manager.Test.Repository.Dapper;

[TestFixture]
[TestOf(typeof(CitaDapperRepository))]
public class CitaDapperRepositoryTest {
    
    private static Cita CrearCita(string matricula = "1111BBB", string dni = "12345678Z", int id = 0) => new Cita {
        Id = id,
        Matricula = matricula,
        Dni = dni,
        Marca = "Skoda",
        Modelo = "Octavia",
        Cilindrada = 2000,
        Motor = Cita.TiposMotor.Diesel,
        FechaMatriculacion = DateTime.Now.AddYears(-20),
        FechaInspeccion = DateTime.Now.AddDays(10),
        IsDeleted = false
    };

    [TestFixture] public class CasosValidos : CitaDapperRepositoryTest {

        private IDbConnection _connection = null!;
        private CitaDapperRepository _repository = null!;

        [SetUp]
        public void SetUp() {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            _repository = new CitaDapperRepository(_connection);
        }

        [TearDown]
        public void TearDown() {
            _connection.Close();
            _connection.Dispose();
        }
        
        [Test]
        public void Constructor_ConDropDataYSeedData_DeberiaInicializarYAgregarSemilla() {
            // act
            using var conn = _connection;
            conn.Open();
            
            var repoConSeed = new CitaDapperRepository(conn, null, true, true);

            // assert
            repoConSeed.CountCita( true).Should().BeGreaterThan(0);
        }
        
        [Test]
        public void GetByIdYCreate_CitaExistente_DeberiaRetornarCita() {
            // arrange
            var cita = CrearCita();
            var creada = _repository.Create(cita).Value;

            // act
            var resultado = _repository.GetById(creada.Id);

            // assert
            resultado.Should().NotBeNull();
            resultado.Matricula.Should().Be(cita.Matricula);
        }
        
        [Test]
        public void GetAll_DeberiaRetornarElementosPaginadosYSinEliminadosPorDefecto() {
            // arrange
            var c1 = _repository.Create(CrearCita("1111AAA")).Value;
            var c2 = _repository.Create(CrearCita("2222BBB")).Value;
            _repository.Delete(c2.Id); // borrado lógico

            // act
            var resultado = _repository.GetAll();

            // assert
            resultado.Should().ContainSingle();
            resultado.First().Id.Should().Be(c1.Id);
        }
        
        [Test]
        public void GetAll_IncluyendoEliminados_DeberiaTraerTodos() {
            // arrange
            var c1 = _repository.Create(CrearCita("1111AAA")).Value;
            var c2 = _repository.Create(CrearCita("2222BBB")).Value;
            _repository.Delete(c2.Id, isLogical: true);

            // act
            var resultado = _repository.GetAll(incluirEliminados: true);

            // assert
            resultado.Should().HaveCount(2);
        }
        
        [Test]
        public void Update_CitaExistenteSinCambios_DeberiaActualizarCorrectamente() {
            // Arrange
            var creada = _repository.Create(CrearCita("1234BBB")).Value;
            var actualizada = creada with { Modelo = "ModeloCambio" };

            // Act
            var resultado = _repository.Update(creada.Id, actualizada);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Modelo.Should().Be("ModeloCambio");
        }

        [Test]
        public void Delete_BorradoFisico_DeberiaQuitarloDeLaBaseDeDatos() {
            // arrange
            var creada = _repository.Create(CrearCita()).Value;

            // act
            _repository.Delete(creada.Id, isLogical: false);

            // assert
            _repository.CountCita(incluirEliminados: true).Should().Be(0);
        }
        
        [Test]
        public void DeleteAll_DeberiaVaciarLaTabla() {
            // arrange
            _repository.Create(CrearCita("1111AAA"));
            _repository.Create(CrearCita("2222BBB"));

            // act
            var resultado = _repository.DeleteAll();

            // assert
            resultado.Should().BeTrue();
            _repository.CountCita(incluirEliminados: true).Should().Be(0);
        }
        
        [Test]
        public void Restore_CitaActiva_DeberiaRetornarLaMismaCitaDirectamente() {
            // arrange
            var creada = _repository.Create(CrearCita()).Value;

            // act
            var resultado = _repository.Restore(creada.Id);

            // assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.IsDeleted.Should().BeFalse();
        }

        [Test]
        public void Restore_CitaEliminadaLogicamente_DeberiaRestaurarla() {
            // arrange
            var creada = _repository.Create(CrearCita()).Value;
            _repository.Delete(creada.Id, isLogical: true);

            // act
            var resultado = _repository.Restore(creada.Id);

            // assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.IsDeleted.Should().BeFalse();
            _repository.GetById(creada.Id).Should().NotBeNull();
        }
        
        [Test]
        public void GetByMatricula_CitaActiva_DeberiaRetornarla() {
            // arrange
            _repository.Create(CrearCita());

            // act
            var resultado = _repository.GetByMatricula("1111BBB");

            // assert
            resultado.Should().NotBeNull();
            resultado.Matricula.Should().Be("1111BBB");
        }
        
        [Test]
        public void GetWithFilters_VariosFiltrosYMotores_DeberiaFiltrarCorrectamente() {
            // arrange
            var fechaFijaInspeccion = DateTime.Today.AddDays(1);
            var fechaFiltroInicio = DateTime.Today.AddDays(-1);
            
            var citaGasolina = CrearCita("1111GAS") with { FechaInspeccion = fechaFijaInspeccion };
            var citaG = citaGasolina with { Motor = Cita.TiposMotor.Gasolina, Marca = "BMW" };
            var citaDiesel = CrearCita("2222DIE") with { FechaInspeccion = fechaFijaInspeccion };
            var citaD = citaDiesel with { Motor = Cita.TiposMotor.Diesel};
            var citaElectrico = CrearCita("3333ELE") with { FechaInspeccion = fechaFijaInspeccion };
            var citaE = citaElectrico with { Motor = Cita.TiposMotor.Electrico};
            var citaHibrido = CrearCita("4444HIB", dni: "12344444G") with { FechaInspeccion = fechaFijaInspeccion };
            var citaH = citaHibrido with { Motor = Cita.TiposMotor.Hibrido};

            _repository.Create(citaG);
            _repository.Create(citaD);
            _repository.Create(citaE);
            _repository.Create(citaH);
            

            // act
            var resGasolina = _repository.GetWithFilters(fechaFiltroInicio, null, 1, 10, "bmw", "gasolina");
            var resDiesel = _repository.GetWithFilters(fechaFiltroInicio, null, 1, 10, null, "diesel");
            var resElec = _repository.GetWithFilters(fechaFiltroInicio, null, 1, 10, null, "electrico");
            var resHib = _repository.GetWithFilters(fechaFiltroInicio, null, 1, 10, null, "hibrido");

            // assertions
            resGasolina.Value.Should().ContainSingle().Which.Matricula.Should().Be("1111GAS");
            resDiesel.Value.Should().ContainSingle().Which.Matricula.Should().Be("2222DIE");
            resElec.Value.Should().ContainSingle().Which.Matricula.Should().Be("3333ELE");
            resHib.Value.Should().ContainSingle().Which.Matricula.Should().Be("4444HIB");
        }
        
        [Test]
        public void GetWithFilters_ConFechaFinYSearchCompleto_DeberiaAcotarResultados()
        {
            // arrange
            var citaCreada = CrearCita("7777FFF");
            var cita = citaCreada with { FechaInspeccion = DateTime.UtcNow.AddDays(2)};
            _repository.Create(cita);

            // act
            var resFueraDeFecha = _repository.GetWithFilters(DateTime.Now.AddDays(5), DateTime.Now.AddDays(10), 1, 10);
            var resFiltroTextoDni = _repository.GetWithFilters(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(5), 1, 10);

            // assert
            resFueraDeFecha.Value.Should().BeEmpty();
            resFiltroTextoDni.Value.Should().ContainSingle();
        }
        [Test]
        public void CountCitasFiltradas_FiltrosDiferentesMotores_DeberiaContarCorrectamente()
        {
            // arrange
            var citaGasolina = CrearCita("1111GAS"); 
            var citaG = citaGasolina with { Motor = Cita.TiposMotor.Gasolina};
            var citaDiesel = CrearCita("2222DIE"); 
            var citaD = citaDiesel with { Motor = Cita.TiposMotor.Diesel};
            _repository.Create(citaG);
            _repository.Create(citaD);

            // act
            var conteoGasolina = _repository.CountCitasFiltradas(null, DateTime.Now.AddDays(-1), null, false, "gasolina");
            var conteoDiesel = _repository.CountCitasFiltradas(null, DateTime.Now.AddDays(-1), null, false, "diesel");
            var conteoElectrico = _repository.CountCitasFiltradas(null, DateTime.Now.AddDays(-1), null, false, "electrico");
            var conteoHibrido = _repository.CountCitasFiltradas(null, DateTime.Now.AddDays(-1), null, false, "hibrido");

            // assert
            conteoGasolina.Should().Be(1);
            conteoDiesel.Should().Be(1);
            conteoElectrico.Should().Be(0);
            conteoHibrido.Should().Be(0);
        }
    }

    
    [TestFixture] 
    public class CasosInvalidos: CitaDapperRepositoryTest {

        private IDbConnection _connection = null!;
        private CitaDapperRepository _repository = null!;
        
        [SetUp]
        public void SetUp() {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            _repository = new CitaDapperRepository(_connection);
        }
        [TearDown]
        public void TearDown() {
            _connection.Close();
            _connection.Dispose();
        }
        
        [Test]
        public void GetById_ConConexionCerrada_DeberiaLanzarErrorYRetornarNull() {
            // arrange
            _connection.Close();

            // act
            var resultado = _repository.GetById(1);

            // assert
            resultado.Should().BeNull();
        }
        
        [Test]
        public void GetAll_ConConexionCerrada_DeberiaRetornarVacio() {
            // arrange
            _connection.Close();

            // act
            var resultado = _repository.GetAll();

            // assert
            resultado.Should().BeEmpty();
        }
        
        [Test]
        public void Create_MismoVehiculoMismoDia_DeberiaRetornarFailurePorRN() {
            // arrange
            var cita1 = CrearCita( "1234BBB");
            _repository.Create(cita1).IsSuccess.Should().BeTrue();

            // misma matrícula y misma fecha
            var citaRepetida = CrearCita("1234BBB");

            // act
            var resultado = _repository.Create(citaRepetida);

            // assert
            resultado.IsFailure.Should().BeTrue();
            (resultado.Error as CitaError.Database)?.Detalles.Should()
                .Contain("El vehículo con matrícula 1234BBB ya tiene una cita asignada para ese día.");
        }
        
        [Test]
        public void Create_ExcedeMaxCitasPorDni_DeberiaRetornarFailurePorRN() {
            // arrange
            var cita1 = CrearCita("1111AAA");
            var cita2 = CrearCita("2222BBB");
            var cita3 = CrearCita("3333CCC");
            var cita4 = CrearCita("4444CCC");


            _repository.Create(cita1).IsSuccess.Should().BeTrue();
            _repository.Create(cita2).IsSuccess.Should().BeTrue();
            _repository.Create(cita3).IsSuccess.Should().BeTrue();

            // act
            var resultado = _repository.Create(cita4);

            // assert
            resultado.IsFailure.Should().BeTrue();
            (resultado.Error as CitaError.Database)?.Detalles.Should()
                .Contain($"El propietario con DNI {cita4.Dni} no puede registrar más de {AppConfig.MaxVehiculosPorDni} citas el mismo día.");
        }

        [Test]
        public void Create_ConConexionCerrada_DeberiaRetornarFailureDeBaseDeDatos() {
            // arrange
            _connection.Close();

            // act
            var resultado = _repository.Create(CrearCita());

            // assert
            resultado.IsFailure.Should().BeTrue();
        }
        
        [Test]
        public void Update_CitaInexistente_DeberiaRetornarFailureNotFound() {
            // act
            var resultado = _repository.Update(50, CrearCita());

            // assert
            resultado.IsFailure.Should().BeTrue();
        }
        
        [Test]
        public void Update_CambiandoMatriculaAFechaOcupada_DeberiaDarErrorPorRN() {
            // Arrange
            var citaExistente = _repository.Create(CrearCita("1111AAA")).Value;
            var otraCita = _repository.Create(CrearCita("2222BBB")).Value;

            // cambio matricula para misma fecha
            var actualizada = citaExistente with { Matricula = "1111AAA" };
            
            // act
            var resultado = _repository.Update(otraCita.Id, actualizada);

            // assert
            resultado.IsFailure.Should().BeTrue();
            (resultado.Error as CitaError.Database)?.Detalles.Should()
                .Contain($"No se puede actualizar: El vehículo con matrícula {citaExistente.Matricula} ya tiene otra cita asignada para ese día.");
        }

        [Test]
        public void Update_CambiandoDniYExcediendoLimite_DeberiaDarErrorPorRN() {
            // arrange
            var c1 = _repository.Create(CrearCita("1111AAA")).Value;
            var c2 = _repository.Create(CrearCita("2222BBB")).Value; 
            var c3 = _repository.Create(CrearCita("3333CCC")).Value;
            var c4 = _repository.Create(CrearCita("4444CCC", "98765432Z")).Value;
            
            var actualizada = c4 with { Dni = "12345678Z" };

            // act
            var resultado = _repository.Update(c4.Id, actualizada);

            // assert
            resultado.IsFailure.Should().BeTrue();
            (resultado.Error as CitaError.Database)?.Detalles.Should()
                .Contain($"No se puede actualizar: El propietario con DNI {actualizada.Dni} ya tiene el límite de {AppConfig.MaxVehiculosPorDni} citas asignadas para ese día.");
        }

        [Test]
        public void Update_ErrorDeConexion_DeberiaRetornarFailure() {
            // arrange
            var creada = _repository.Create(CrearCita()).Value;
            _connection.Close(); 

            // act
            var resultado = _repository.Update(creada.Id, creada);

            // assert
            resultado.IsFailure.Should().BeTrue();
        }
        
        [Test]
        public void Delete_CitaInexistente_DeberiaRetornarNull() {
            // act
            var resultado = _repository.Delete(90);

            // assert
            resultado.Should().BeNull();
        }
        
        [Test]
        public void Delete_ConErrorDeConexion_DeberiaRetornarNull() {
            // arrange
            var creada = _repository.Create(CrearCita()).Value;
            _connection.Close();
            
            // act
            var resultado = _repository.Delete(creada.Id);

            // assert
            resultado.Should().BeNull();
        }
        
        [Test]
        public void DeleteAll_ConConexionCerrada_DeberiaRetornarFalse() {
            // arrange
            _connection.Close();

            // act
            var resultado = _repository.DeleteAll();

            // assert
            resultado.Should().BeFalse();
        }
        
        [Test]
        public void Restore_CitaInexistente_DeberiaRetornarFailure() {
            // act
            var resultado = _repository.Restore(80);

            // assert
            resultado.IsFailure.Should().BeTrue();
        }
        
        [Test]
        public void Restore_VehiculoYaTieneOtraCitaActivaEseDia_DeberiaRetornarFailurePorRN() {
            
            var citaParaBorrar = _repository.Create(CrearCita("1234BBB")).Value;
            // eliminacion logica
            _repository.Delete(citaParaBorrar.Id, isLogical: true);
            
            var citaActivaBloqueante = CrearCita("1234BBB");
            _repository.Create(citaActivaBloqueante).IsSuccess.Should().BeTrue();

            // act
            var resultado = _repository.Restore(citaParaBorrar.Id);

            // assert
            resultado.IsFailure.Should().BeTrue();
            (resultado.Error as CitaError.Database)?.Detalles.Should()
                .Contain($"No se puede restaurar: El vehículo ya cuenta con otra cita activa ese mismo día.");
        }

        [Test]
        public void Restore_ConConexionCerrada_DeberiaRetornarFailure() {
            // arrange
            var creada = _repository.Create(CrearCita()).Value;
            _repository.Delete(creada.Id, isLogical: true);
            _connection.Close();

            // act
            var resultado = _repository.Restore(creada.Id);

            // assert
            resultado.IsFailure.Should().BeTrue();
        }
        
        [Test]
        public void GetByMatricula_ExcepcionConexion_DeberiaRetornarNull() {
            // arrange
            _connection.Close();

            // act
            var resultado = _repository.GetByMatricula("1111BBB");

            // assert
            resultado.Should().BeNull();
        }
        
        [Test]
        public void CountCita_ExcepcionConexion_DeberiaRetornarCero() {
            // arrange
            _connection.Close();

            // act
            var resultado = _repository.CountCita();

            // assert
            resultado.Should().Be(0);
        }
        
        [Test]
        public void GetWithFilters_ConConexionCerrada_DeberiaRetornarFailure() {
            // arrange
            _connection.Close();

            // act
            var resultado = _repository.GetWithFilters(DateTime.Now, null, 1, 10);

            // assert
            resultado.IsFailure.Should().BeTrue();
        }
        [Test]
        public void CountCitasFiltradas_ConConexionCerrada_DeberiaRetornarCero() {
            // arrange
            _connection.Close();

            // act
            var resultado = _repository.CountCitasFiltradas(null, DateTime.Now, null, false);

            // assert
            resultado.Should().Be(0);
        }
    }
}