using FluentAssertions;
using Manager.Config;
using Manager.Errors.Citas;
using Manager.Factories;
using Manager.Models;
using Manager.Repositories.Ado;
using Microsoft.Data.Sqlite;

namespace Manager.Test.Repository.Ado;

[TestFixture]
[TestOf(typeof(CitaAdoRepository))]
public class CitaAdoRepositoryTest {
    
    private CitaAdoRepository _repository;

    [SetUp]
    public void SetUp() {
        _repository = new CitaAdoRepository(true, false);
    }

    private static Cita CrearCita(string matricula = "1111BBB", string dni = "12345678Z", int id = 0) => new() {
        Id = id,
        Matricula = matricula,
        Dni = dni,
        Marca = "Skoda",
        Modelo = "Octavia",
        Cilindrada = 2000,
        Motor = Cita.TiposMotor.Diesel,
        FechaMatriculacion = DateTime.Today.AddYears(-20),
        FechaInspeccion = DateTime.Today.AddDays(10),
        IsDeleted = false
    };

    [Test]
    public void Constructor_DebeInsertarSedd() {
        var rep = new CitaAdoRepository(true, true);

        rep.CountCita(true).Should().Be(CitaFactory.Seed().Count());

    }

    [Test]
    public void Create_DebeInsertarEnBaseDeDatosYRecuperar() {
        // arrange y act
        var result = _repository.Create(CrearCita());

        // assert
        result.IsSuccess.Should().BeTrue();
        var recuperado = _repository.GetById(result.Value.Id);
        recuperado.Should().NotBeNull();
        recuperado.Matricula.Should().Be("1111BBB");
    }
    
    [Test]
    public void Create_CuandoVehiculoYaTieneCitaMismoDia_RetornaFailure() {
        
        // arrange
        _repository.Create(CrearCita(dni: "12345678L"));

        // act
        var result = _repository.Create(CrearCita(dni: "98765432S")); 

        // assert
        result.IsFailure.Should().BeTrue();
        (result.Error as CitaError.Database)?.Detalles.Should()
            .Contain($"El vehículo con matrícula 1111BBB ya tiene una cita asignada para ese día.");
    }

    [Test]
    public void Create_ExcederLimiteCitasPorDniMismoDia_DebeRetornarError() {
        
        // arrange
        _repository.Create(CrearCita("1234CCC" ));
        _repository.Create(CrearCita("1234DDD"));
        _repository.Create(CrearCita("1111BBC"));


        // act
        var c4 = CrearCita("9999XXX");
        var result = _repository.Create(c4);

        // assert
        result.IsFailure.Should().BeTrue();
        (result.Error as CitaError.Database)?.Detalles.Should()
            .Contain($"El propietario con DNI {c4.Dni} no puede registrar más de {AppConfig.MaxVehiculosPorDni} citas el mismo día.");
    }
    
    [Test]
    public void Update_CambiarModeloSinCambiarFechaNiMatricula_DebeTenerExito() {
        // arrange
        var original = CrearCita("9999XXX");
        var creada = _repository.Create(original).Value;

        // act
        var editada = creada with { Modelo = "Focus", Marca = "Ford" };
        var result = _repository.Update(creada.Id, editada);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Modelo.Should().Be("Focus");
    }
    
    [Test]
    public void Update_CambiandoMatriculaAFechaYaOcupadaPorOtroCoche_DeberiaDarError() {
        // arrange
        var citaCocheA = _repository.Create(CrearCita("1111AAA")).Value;
        var citaCocheB = _repository.Create(CrearCita("2222BBB")).Value;
        
        var modificacionInvalida = citaCocheB with { Matricula = "1111AAA" };
        
        // act
        var result = _repository.Update(citaCocheB.Id, modificacionInvalida);

        // assert
        result.IsFailure.Should().BeTrue();
        (result.Error as CitaError.Database)?.Detalles.Should()
            .Contain($"No se puede actualizar: El vehículo con matrícula {citaCocheA.Matricula} ya tiene otra cita asignada para ese día.");
    }
    
    [Test]
    public void Update_CambiandoDni_DeberiaDarError() {
        // arrange
        var citaCocheA = _repository.Create(CrearCita("1111AAA")).Value;
        var citaCocheB = _repository.Create(CrearCita("2222BBB")).Value;
        var citaCocheC = _repository.Create(CrearCita("3333CCC")).Value;
        var citaCocheD = _repository.Create(CrearCita("4444JJJ", "12345678J")).Value;

        
        var modificacionInvalida = citaCocheD with { Dni = "12345678Z" };
        
        // act
        var result = _repository.Update(citaCocheD.Id, modificacionInvalida);

        // assert
        result.IsFailure.Should().BeTrue();
        (result.Error as CitaError.Database)?.Detalles.Should()
            .Contain($"No se puede actualizar: El propietario con DNI {modificacionInvalida.Dni} ya tiene el límite de {AppConfig.MaxVehiculosPorDni} citas asignadas para ese día.");
    }
    
    [Test]
    public void Update_CambiandoCitaInvalida_DeberiaDarError() {
        // arrange y act
        var result = _repository.Update(23123, CrearCita());

        // assert
        result.IsFailure.Should().BeTrue();
        (result.Error as CitaError.Database)?.Detalles.Should()
            .Contain($"ya tiene el límite de {AppConfig.MaxVehiculosPorDni} citas asignadas para ese día.");
    }
    
    [Test]
    public void Delete_Logico_DebeMarcarComoBorradoYEsconderloDeGetAll() {
        // arrange
        var creada = _repository.Create(CrearCita()).Value;

        // act
        _repository.Delete(creada.Id, isLogical: true);
        
        var recuperado = _repository.GetById(creada.Id);
        var listadoActivos = _repository.GetAll(pagina: 1, tamPagina: 10, incluirEliminados: false);

        // assert
        recuperado.Should().NotBeNull();
        recuperado.IsDeleted.Should().BeTrue(); 
        listadoActivos.Should().BeEmpty();    
    }
    
    [Test]
    public void Delete_Fisico_DebeMarcarComoBorradoYEsconderloDeGetAll() {
        // arrange
        var creada = _repository.Create(CrearCita()).Value;

        // act
        _repository.Delete(creada.Id, isLogical: false);
        
        var recuperado = _repository.GetById(creada.Id);

        // assert
        recuperado.Should().BeNull();
    }
    
    [Test]
    public void Delete_CitaNull_DebeRetornarNull() {
        // arrange y act
        var borrada = _repository.Delete(999, isLogical: true);
        
        // assert
        borrada.Should().BeNull();
    }
    
    [Test]
    public void DeleteAll_DebeVaciarLaTablaPorCompleto() {
        // arrange
        _repository.Create(CrearCita());
        _repository.Create(CrearCita("1112CCC"));

        // act
        var resultado = _repository.DeleteAll();

        // assert
        resultado.Should().BeTrue();
        _repository.CountCita(incluirEliminados: true).Should().Be(0);
    }
    
    [Test]
    public void GetByMatricula_DebeRetornarCorrecto() {
        // arrange
        _repository.Create(CrearCita());

        // act
        var encontrado = _repository.GetByMatricula("1111BBB");

        // assert
        encontrado.Should().NotBeNull();
        encontrado.Matricula.Should().Be("1111BBB");
    }
    
    [Test]
    public void GetByMatricula_MatriculaInexistente_DebeRetornarNull() {
        // arrange
        _repository.Create(CrearCita());

        // act
        var encontrado = _repository.GetByMatricula("dksagdhksad");

        // assert
        encontrado.Should().BeNull();
    }
    [Test]
    public void GetWithFilters_PorTextoYTiposDeMotor_DebeAcotarResultados() {
        // arrange
        _repository.Create(CrearCita("1111GAS") with { FechaInspeccion = DateTime.Today, Motor = Cita.TiposMotor.Gasolina, Marca = "BMW" });
        _repository.Create(CrearCita("2222DIE") with { FechaInspeccion = DateTime.Today, Motor = Cita.TiposMotor.Diesel, Marca = "Seat" });

        // act
        var filtroMotor = _repository.GetWithFilters(DateTime.Today.AddDays(-1), null, 1, 10, searchText: null, motorSeleccionado: "gasolina");
        var filtroTexto = _repository.GetWithFilters(DateTime.Today.AddDays(-1), null, 1, 10, searchText: "seat", motorSeleccionado: "todos");
        
        // assert
        filtroMotor.Value.Should().ContainSingle().Which.Matricula.Should().Be("1111GAS");
        filtroTexto.Value.Should().ContainSingle().Which.Matricula.Should().Be("2222DIE");
    }
    
    [Test]
    public void Restore_DebeReactivarCitaBorradaLogicamente() {
        // arrange
        var creada = _repository.Create(CrearCita()).Value;
        _repository.Delete(creada.Id, isLogical: true);

        // act
        var result = _repository.Restore(creada.Id);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsDeleted.Should().BeFalse();
        
        var chequeoDb = _repository.GetById(creada.Id);
        chequeoDb!.IsDeleted.Should().BeFalse();
    }
    
    [Test]
    public void Restore_CitaNoBorrada_DeberiaDevolverSuccess() {
        // arrange
        var creada = _repository.Create(CrearCita()).Value;

        // act
        var result = _repository.Restore(creada.Id);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsDeleted.Should().BeFalse();
    }
    
    [Test]
    public void Restore_CitaNoEncontrada_DeberiaDevolverErrorDatabase() {
        // arrange y act
        var result = _repository.Restore(32131);

        // assert
        result.IsFailure.Should().BeTrue();
        (result.Error as CitaError.Database)?.Detalles.Should()
            .Contain($"No se puede restaurar una Cita que no existe.");
    }
    
    [Test]
    public void CountCitasFiltradas_SinFiltros_DebeRetornarElTotalDeActivos() {
        // arrange
        _repository.Create(CrearCita("1111AAA") with { FechaInspeccion = DateTime.Today });
        _repository.Create(CrearCita("2222BBB") with { FechaInspeccion = DateTime.Today });
        var borrada = _repository.Create(CrearCita("3333CCC") with { FechaInspeccion = DateTime.Today }).Value;
        _repository.Delete(borrada.Id, isLogical: true);

        // act
        var totalActivos = _repository.CountCitasFiltradas( null,  DateTime.Today.AddDays(-1), null, false,  "todos");
        var totalConEliminados = _repository.CountCitasFiltradas( null,  DateTime.Today.AddDays(-1),  null,  true, "todos");

        // assert
        totalActivos.Should().Be(2);
        totalConEliminados.Should().Be(3); 
    }
    
    [Test]
    public void CountCitasFiltradas_FiltrosDeMotorEspecificos_DebePasarPorCadaIfYContarCorrectamente() {
        // arrange
        var hoy = DateTime.Today;
        _repository.Create(CrearCita("1111GAS") with { FechaInspeccion = hoy, Motor = Cita.TiposMotor.Gasolina });
        _repository.Create(CrearCita("2222DIE") with { FechaInspeccion = hoy, Motor = Cita.TiposMotor.Diesel });
        _repository.Create(CrearCita("3333ELE") with { FechaInspeccion = hoy, Motor = Cita.TiposMotor.Electrico });
        _repository.Create(CrearCita("4444HIB") with { FechaInspeccion = hoy, Motor = Cita.TiposMotor.Hibrido, Dni = "23456789T"});

        // act
        var conteoGasolina = _repository.CountCitasFiltradas(null, hoy.AddDays(-1), null, false, "gasolina");
        var conteoDiesel = _repository.CountCitasFiltradas(null, hoy.AddDays(-1), null, false, "diesel");
        var conteoElectrico = _repository.CountCitasFiltradas(null, hoy.AddDays(-1), null, false, "electrico");
        var conteoHibrido = _repository.CountCitasFiltradas(null, hoy.AddDays(-1), null, false, "hibrido");
        
        // assert
        conteoDiesel.Should().Be(1);
        conteoElectrico.Should().Be(1);
        conteoHibrido.Should().Be(1);
        conteoGasolina.Should().Be(1);
    }
    
    [Test]
    public void GetWithFilters_FiltrosDeMotorEspecificos_DebePasarPorCadaIfYContarCorrectamente() {
        // arrange
        var hoy = DateTime.Today;
        _repository.Create(CrearCita("1111GAS") with { FechaInspeccion = hoy, Motor = Cita.TiposMotor.Gasolina });
        _repository.Create(CrearCita("2222DIE") with { FechaInspeccion = hoy, Motor = Cita.TiposMotor.Diesel });
        _repository.Create(CrearCita("3333ELE") with { FechaInspeccion = hoy, Motor = Cita.TiposMotor.Electrico });
        _repository.Create(CrearCita("4444HIB") with { FechaInspeccion = hoy, Motor = Cita.TiposMotor.Hibrido, Dni = "23456789T"});

        // act
        var citasGasolina = _repository.GetWithFilters(hoy.AddDays(-1), null, 1, 10, null, "gasolina");
        var citasDiesel = _repository.GetWithFilters(hoy.AddDays(-1), null, 1, 10, null, "diesel");
        var citasElectrico = _repository.GetWithFilters(hoy.AddDays(-1), null, 1, 10, null, "electrico");
        var citasHibrido = _repository.GetWithFilters(hoy.AddDays(-1), null, 1, 10, null, "hibrido");
        
        // assert
        citasGasolina.IsSuccess.Should().BeTrue();
        citasGasolina.Value.Should().ContainSingle().Which.Matricula.Should().Be("1111GAS");
        
        citasDiesel.IsSuccess.Should().BeTrue();
        citasDiesel.Value.Should().ContainSingle().Which.Matricula.Should().Be("2222DIE");
        
        citasElectrico.IsSuccess.Should().BeTrue();
        citasElectrico.Value.Should().ContainSingle().Which.Matricula.Should().Be("3333ELE");
        
        citasHibrido.IsSuccess.Should().BeTrue();
        citasHibrido.Value.Should().ContainSingle().Which.Matricula.Should().Be("4444HIB");
    }
}