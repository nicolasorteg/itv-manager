using CSharpFunctionalExtensions;
using FluentAssertions;
using Manager.Cache.Common;
using Manager.Config;
using Manager.Errors.Citas;
using Manager.Errors.Common;
using Manager.Models;
using Manager.Repositories.Base;
using Manager.Service.Manager;
using Manager.Validators.Common;
using Moq;

namespace Manager.Test.Service.Manager;

[TestFixture]
[TestOf(typeof(ManagerService))]
public class ManagerServiceTest {
    
    private Mock<ICitaRepository> _citaRepositoryMock;
    private Mock<IValidator<Cita>> _citaValidatorMock;
    private Mock<ICache<int, Cita>> _cacheMock;
    private ManagerService _managerService;
    private Cita _citaPrueba;

    [SetUp]
    public void Setup() {

        _citaRepositoryMock = new Mock<ICitaRepository>();
        _citaValidatorMock =  new Mock<IValidator<Cita>>();
        _cacheMock =  new Mock<ICache<int, Cita>>();
        _managerService = new ManagerService(_citaRepositoryMock.Object, _citaValidatorMock.Object, _cacheMock.Object);

        // objeto de prueba
        _citaPrueba = new Cita {
            Id = 1,
            Matricula = "1111bbb",
            Dni = "12345678Z",
            Marca = "Sokda",
            Modelo = "Octavia",
            Cilindrada = 2000,
            Motor = Cita.TiposMotor.Hibrido,
            FechaMatriculacion = DateTime.UtcNow.AddYears(-20),
            FechaInspeccion = DateTime.Today.AddDays(14)
        };
    }
    
    [Test]
    public void ObtenerPorId_SiEstaEnCache_DeberiaDevolverloSinLlamarAlRepositorio() {
        // arrange
        _cacheMock.Setup(c =>c.Get(_citaPrueba.Id)).Returns(_citaPrueba);

        // act
        var resultado = _managerService.ObtenerPorId(_citaPrueba.Id);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(_citaPrueba);
        _cacheMock.Verify(c => c.Get(_citaPrueba.Id), Times.Once);
        _citaRepositoryMock.Verify(r => r.GetById(It.IsAny<int>()), Times.Never);
    }
    
    [Test]
    public void ObtenerPorId_SiNoEstaEnCache_DeberiaBuscarEnRepoYGuardarEnCache() {
        // arrange
        _cacheMock.Setup(c => c.Get(_citaPrueba.Id)).Returns((Cita?)null);
        _citaRepositoryMock.Setup(r => r.GetById(_citaPrueba.Id)).Returns(_citaPrueba);

        // act
        var resultado = _managerService.ObtenerPorId(_citaPrueba.Id);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(_citaPrueba);
        _cacheMock.Verify(c => c.Get(_citaPrueba.Id), Times.Once);
        _citaRepositoryMock.Verify(r => r.GetById(_citaPrueba.Id), Times.Once);
        _cacheMock.Verify(c => c.Add(_citaPrueba.Id, _citaPrueba), Times.Once);
    }
    
    [Test]
    public void CrearCita_SiPasaTodasLasReglas_DeberiaInsertarYActualizarCache() {
        // arrange
        _citaValidatorMock.Setup(v => v.Validar(_citaPrueba)).Returns(CSharpFunctionalExtensions.Result.Success<Cita, DomainError>(_citaPrueba));
        _citaRepositoryMock.Setup(r => r.ExisteCitaParaVehiculoEnFecha(_citaPrueba.Matricula, _citaPrueba.FechaInspeccion)).Returns(false);
        _citaRepositoryMock.Setup(r => r.ContarCitasPropietarioEnFecha(_citaPrueba.Dni, _citaPrueba.FechaInspeccion)).Returns(0);
        _citaRepositoryMock.Setup(r => r.Create(_citaPrueba)).Returns(CSharpFunctionalExtensions.Result.Success<Cita, DomainError>(_citaPrueba));

        // act
        var resultado = _managerService.CrearCita(_citaPrueba);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(_citaPrueba);
        _citaRepositoryMock.Verify(r => r.Create(_citaPrueba), Times.Once);
        _cacheMock.Verify(c => c.Add(_citaPrueba.Id, _citaPrueba), Times.Once);
    }
    
    [Test]
    public void CrearCita_SiVehiculoYaTieneCitaEseDia_DeberiaFrenarPorRN05() {
        // arrange
        _citaValidatorMock.Setup(v => v.Validar(_citaPrueba)).Returns(CSharpFunctionalExtensions.Result.Success<Cita, DomainError>(_citaPrueba));
        _citaRepositoryMock.Setup(r => r.ExisteCitaParaVehiculoEnFecha(_citaPrueba.Matricula, _citaPrueba.FechaInspeccion)).Returns(true);

        // act
        var resultado = _managerService.CrearCita(_citaPrueba);

        // assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<CitaError.InspeccionRepetida>();
        _citaRepositoryMock.Verify(r => r.Create(It.IsAny<Cita>()), Times.Never);
        _cacheMock.Verify(c => c.Add(It.IsAny<int>(), It.IsAny<Cita>()), Times.Never);
    }

    [Test]
    public void CrearCita_SiDniSuperaLimiteMaximo_DeberiaFrenarPorRN06() {
        // arrange
        _citaValidatorMock.Setup(v => v.Validar(_citaPrueba)).Returns(CSharpFunctionalExtensions.Result.Success<Cita, DomainError>(_citaPrueba));
        _citaRepositoryMock.Setup(r => r.ExisteCitaParaVehiculoEnFecha(_citaPrueba.Matricula, _citaPrueba.FechaInspeccion)).Returns(false);
        _citaRepositoryMock.Setup(r => r.ContarCitasPropietarioEnFecha(_citaPrueba.Dni, _citaPrueba.FechaInspeccion)).Returns(AppConfig.MaxVehiculosPorDni);

        // act
        var resultado = _managerService.CrearCita(_citaPrueba);

        // assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<CitaError.MaximosVehiculosAlcanzados>();
        _citaRepositoryMock.Verify(r => r.Create(It.IsAny<Cita>()), Times.Never);
    }
    
    [Test]
    public void ActualizarCita_SiExiste_DeberiaModificarYGuardarEnCache() {
        // arrange
        _citaValidatorMock.Setup(v => v.Validar(_citaPrueba)).Returns(CSharpFunctionalExtensions.Result.Success<Cita, DomainError>(_citaPrueba));
        _citaRepositoryMock.Setup(r => r.GetById(_citaPrueba.Id)).Returns(_citaPrueba);
        _citaRepositoryMock.Setup(r => r.Update(_citaPrueba.Id, _citaPrueba)).Returns(CSharpFunctionalExtensions.Result.Success<Cita, DomainError>(_citaPrueba));

        // act
        var resultado = _managerService.ActualizarCita(_citaPrueba.Id, _citaPrueba);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        _citaRepositoryMock.Verify(r => r.Update(_citaPrueba.Id, _citaPrueba), Times.Once);
        _cacheMock.Verify(c => c.Add(_citaPrueba.Id, _citaPrueba), Times.Once);
    }
    
    [Test]
    public void EliminarCita_SiExiste_DeberiaQuitarDeRepoYDeCache() {
        // arrange
        _citaRepositoryMock.Setup(r => r.Delete(_citaPrueba.Id, true)).Returns(_citaPrueba);

        // act
        var resultado = _managerService.EliminarCita(_citaPrueba.Id, true);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(_citaPrueba);
        _citaRepositoryMock.Verify(r => r.Delete(_citaPrueba.Id, true), Times.Once);
        _cacheMock.Verify(c => c.Remove(_citaPrueba.Id), Times.Once);
    }

    [Test]
    public void EliminarCita_SiNoExiste_DeberiaRetornarNotFound() {
        // arrange
        _citaRepositoryMock.Setup(r => r.Delete(_citaPrueba.Id, true)).Returns((Cita?)null);

        // act
        var resultado = _managerService.EliminarCita(_citaPrueba.Id, true);

        // assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<CitaError.NotFound>();
        _cacheMock.Verify(c => c.Remove(It.IsAny<int>()), Times.Never);
    }
    
    [Test]
    public void ObtenerPorMatricula_SiExiste_DeberiaDevolverCita() {
        // arrange
        _citaRepositoryMock.Setup(r => r.GetByMatricula(_citaPrueba.Matricula)).Returns(_citaPrueba);

        // act
        var resultado = _managerService.ObtenerPorMatricula(_citaPrueba.Matricula);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(_citaPrueba);
        _citaRepositoryMock.Verify(r => r.GetByMatricula(_citaPrueba.Matricula), Times.Once);
    }

    [Test]
    public void ObtenerPorMatricula_SiNoExiste_DeberiaRetornarNotFound() {
        // arrange
        _citaRepositoryMock.Setup(r => r.GetByMatricula(_citaPrueba.Matricula)).Returns((Cita?)null);

        // act
        var resultado = _managerService.ObtenerPorMatricula(_citaPrueba.Matricula);

        // assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<CitaError.NotFound>();
    }
    
    [Test]
    public void ObtenerConFiltros_DeberiaLlamarAlRepositorioConParametros() {
        // arrange
        var fechaInicio = DateTime.Today;
        var listaCitas = new List<Cita> { _citaPrueba };
        _citaRepositoryMock.Setup(r => r.GetWithFilters(fechaInicio, null, 1, 10, null, "todos", false))
            .Returns(Result.Success<IEnumerable<Cita>, DomainError>(listaCitas));

        // act
        var resultado = _managerService.ObtenerConFiltros(fechaInicio, null, 1, 10, null, "todos", false);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().BeEquivalentTo(listaCitas);
        _citaRepositoryMock.Verify(r => r.GetWithFilters(fechaInicio, null, 1, 10, null, "todos", false), Times.Once);
    }
    
    [Test]
    public void CrearCita_SiFallaLaValidacion_DeberiaFrenarInmediatamente() {
        // arrange
        var listadoErrores = new List<string>();
        var errores = new CitaError.Validation(listadoErrores);
        _citaValidatorMock.Setup(v => v.Validar(_citaPrueba)).Returns(Result.Failure<Cita, DomainError>(errores));

        // act
        var resultado = _managerService.CrearCita(_citaPrueba);

        // assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(errores);
        // si el validador falla no se comprueba ninguna RN en el repositorio
        _citaRepositoryMock.Verify(r => r.ExisteCitaParaVehiculoEnFecha(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        _citaRepositoryMock.Verify(r => r.Create(It.IsAny<Cita>()), Times.Never);
    }
    
    [Test]
    public void ActualizarCita_SiNoExisteEnBd_DeberiaFrenarYRetornarNotFound() {
        // arrange
        _citaValidatorMock.Setup(v => v.Validar(_citaPrueba)).Returns(Result.Success<Cita, DomainError>(_citaPrueba));
        _citaRepositoryMock.Setup(r => r.GetById(_citaPrueba.Id)).Returns((Cita?)null);

        // act
        var resultado = _managerService.ActualizarCita(_citaPrueba.Id, _citaPrueba);

        // assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<CitaError.NotFound>();
        _citaRepositoryMock.Verify(r => r.Update(It.IsAny<int>(), It.IsAny<Cita>()), Times.Never);
    }
    
    [Test]
    public void RestaurarCita_DeberiaLlamarAlRepoYActualizarCache() {
        // arrange
        _citaRepositoryMock.Setup(r => r.Restore(_citaPrueba.Id)).Returns(Result.Success<Cita, DomainError>(_citaPrueba));

        // act
        var resultado = _managerService.RestaurarCita(_citaPrueba.Id);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        _citaRepositoryMock.Verify(r => r.Restore(_citaPrueba.Id), Times.Once);
        _cacheMock.Verify(c => c.Add(_citaPrueba.Id, _citaPrueba), Times.Once);
    }
    
    [Test]
    public void EliminarTodasLasCitas_SiEsExitoso_DeberiaPurgarLaCache() {
        // arrange
        _citaRepositoryMock.Setup(r => r.DeleteAll()).Returns(true);

        // act
        var resultado = _managerService.EliminarTodasLasCitas();

        // assert
        resultado.Should().BeTrue();
        _citaRepositoryMock.Verify(r => r.DeleteAll(), Times.Once);
        _cacheMock.Verify(c => c.Clear(), Times.Once); 
    }

    [Test]
    public void Contadores_DeberianPasarDatosDirectosDelRepo() {
        // arrange
        _citaRepositoryMock.Setup(r => r.CountCitasFiltradas("skoda", DateTime.Today, null, false, "todos")).Returns(5);
        _citaRepositoryMock.Setup(r => r.CountCita(false)).Returns(10);

        // act
        var filtradas = _managerService.ContarCitasFiltradas("skoda", DateTime.Today, null, false);
        var totales = _managerService.ContarTotalCitas();

        // assert
        filtradas.Should().Be(5);
        totales.Should().Be(10);
    }
    
    [Test]
    public void ObtenerPorId_SiNoExisteEnRepo_DeberiaRetornarNotFound() {
        // arrange
        _cacheMock.Setup(c => c.Get(3)).Returns((Cita?)null);
        _citaRepositoryMock.Setup(r => r.GetById(3)).Returns((Cita?)null);

        // act
        var resultado = _managerService.ObtenerPorId(3);

        // assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().BeOfType<CitaError.NotFound>();
        _cacheMock.Verify(c => c.Add(It.IsAny<int>(), It.IsAny<Cita>()), Times.Never);
    }
}