using FluentAssertions;
using Manager.Config;
using Manager.Entity;
using Manager.Errors.Citas;
using Manager.Models;
using Manager.Repositories.EfCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Manager.Test.Repository.EfCore;

[TestFixture]
[TestOf(typeof(CitaEfCoreRepository))]
public class CitaEfCoreRepositoryTest {
    
    private AppDbContext _context = null!;
    private CitaEfCoreRepository _repository = null!;
    private SqliteConnection _connection = null!;

    [SetUp]
    public void SetUp() {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new CitaEfCoreRepository(_context, true);
    }

    [TearDown]
    public void TearDown() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
    
    [Test]
    public void GetById_CuandoExiste_DebeRetornarModelo() {
        // arrange
        var entity = new CitaEntity { Id = 99, Matricula = "1234ABC", Dni = "111A", FechaInspeccion = DateTime.Now, Marca = "Seat", Modelo = "Ibiza" };
        _context.Citas.Add(entity);
        _context.SaveChanges();

        // act
        var resultado = _repository.GetById(99);

        // assert
        resultado.Should().NotBeNull();
        resultado.Matricula.Should().Be("1234ABC");
    }
    
    [Test]
    public void GetAll_DebePaginarseYFiltrarEliminadosLogicos() {
        // arrange
        _context.Citas.AddRange(new List<CitaEntity> {
            new() { Id = 1, Matricula = "M1", Dni = "D1", IsDeleted = false },
            new() { Id = 2, Matricula = "M2", Dni = "D2", IsDeleted = false },
            new() { Id = 3, Matricula = "M3", Dni = "D3", IsDeleted = true } // logical delete
        });
        _context.SaveChanges();

        // act
        var resultado = _repository.GetAll().ToList();
        var resultadoEliminados = _repository.GetAll(incluirEliminados: true).ToList();

        // assert
        resultado.Should().HaveCount(2);
        resultado.First().Id.Should().Be(1);
        resultadoEliminados.Should().HaveCount(3);
    }
    
    [Test]
    public void Create_CuandoTodoEsValido_DebeInsertarCorrectamente() {
        // arrange
        var cita = new Cita { Matricula = "2627KKK", Dni = "12345678Z", FechaInspeccion = DateTime.Today, Marca = "Skoda", Modelo = "Octavia" };

        // act
        var resultado = _repository.Create(cita);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Id.Should().BeGreaterThan(0);
        _context.Citas.Count().Should().Be(1);
    }
    
    [Test]
    public void Create_VehiculoYaTieneCitaMismoDia_DebeRetornarFailure() {
        // arrange
        _context.Citas.Add(new CitaEntity { Matricula = "1111aaa", FechaInspeccion = DateTime.Today, Dni = "11111111Z", Marca = "Skoda", Modelo = "Octavia" });
        _context.SaveChanges();

        var nuevaCita = new Cita { Matricula = "1111aaa", FechaInspeccion = DateTime.Today, Dni = "11111111Z" };

        // act
        var resultado = _repository.Create(nuevaCita);

        // assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Mensaje.Should().Contain($"El vehículo con matrícula {nuevaCita.Matricula} ya tiene una cita asignada para ese día.");
    }

    [Test]
    public void Create_PropietarioSuperaLimiteMaximoDeCitasDia_DebeRetornarFailure() {
        // arrange
        _context.Citas.AddRange(new List<CitaEntity> {
            new() { Matricula = "1111ccc", Dni = "12345678Z", FechaInspeccion = DateTime.Today, Marca = "Skoda", Modelo = "Octavia" },
            new() { Matricula = "2222vvv", Dni = "12345678Z", FechaInspeccion = DateTime.Today, Marca = "Fiat", Modelo = "Punto" },
            new() { Matricula = "2222zzz", Dni = "12345678Z", FechaInspeccion = DateTime.Today, Marca = "Citrone", Modelo = "c4" },
        });
        _context.SaveChanges();

        var cuarta = new Cita { Matricula = "8888fff", Dni = "12345678Z", FechaInspeccion = DateTime.Today };

        // act
        var resultado = _repository.Create(cuarta);

        // assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Mensaje.Should().Contain($"El propietario con DNI 12345678Z no puede registrar más de {AppConfig.MaxVehiculosPorDni} citas el mismo día.");
    }
    
    [Test]
    public void Update_CitaExistenteSinCambios_DeberiaActualizarCorrectamente() {
        // arrange
        var c1 = new Cita { Matricula = "1111aaa", FechaInspeccion = DateTime.Today, Dni = "11111111Z" };
        
        var creada = _repository.Create(c1).Value;
        var actualizada = creada with { Modelo = "ModeloCambio" };

        // act
        var resultado = _repository.Update(creada.Id, actualizada);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Modelo.Should().Be("ModeloCambio");
    }
    
    [Test]
    public void Update_CitaInexistente_DeberiaFallar() {

        // act
        var resultado = _repository.Update(1, new Cita { Matricula = "1111aaa", FechaInspeccion = DateTime.Today, Dni = "11111111Z" });

        // assert
        resultado.IsSuccess.Should().BeFalse();
        (resultado.Error as CitaError.NotFound)?.Id.Should()
            .Contain("No existe la cita con ID 1 para actualizar.");
    }
    
    [Test]
    public void Update_CambiandoMatriculaAFechaOcupada_DeberiaDarErrorPorRN() {
        // arrange
        var citaExistente = _repository.Create(new Cita { Matricula = "1111aaa", FechaInspeccion = DateTime.Today, Dni = "11111111Z" }).Value;
        var otraCita = _repository.Create(new Cita { Matricula = "2222bbb", FechaInspeccion = DateTime.Today, Dni = "34567891D" }).Value;

        // cambio matricula para misma fecha
        var actualizada = otraCita with { Matricula = "1111aaa" };
            
        // act
        var resultado = _repository.Update(otraCita.Id, actualizada);

        // assert
        resultado.IsFailure.Should().BeTrue();
        (resultado.Error as CitaError.Database)?.Detalles.Should()
            .Contain($"No se puede actualizar: El vehículo con matrícula {citaExistente.Matricula} ya tiene otra cita asignada para ese día.");
    }
    
    [Test]
    public void Update_CambiandoMatriculaAFechaNoOcupada_DeberiaDarSuccess() {
        // arrange
        var citaExistente = _repository.Create(new Cita { Matricula = "1111aaa", FechaInspeccion = DateTime.Today, Dni = "11111111Z" }).Value;
        var otraCita = _repository.Create(new Cita { Matricula = "2222bbb", FechaInspeccion = DateTime.UtcNow.AddDays(10), Dni = "34567891D" }).Value;

        // cambio matricula para misma fecha
        var actualizada = otraCita with { Matricula = "1111aaa" };
            
        // act
        var resultado = _repository.Update(otraCita.Id, actualizada);

        // assert
        resultado.IsFailure.Should().BeFalse();
    }

    [Test]
    public void Update_CambiandoDniYExcediendoLimite_DeberiaDarErrorPorRN() {
        // arrange
        var c1 = _repository.Create(new Cita { Matricula = "1111aaa", FechaInspeccion = DateTime.Today, Dni = "11111111Z" }).Value;
        var c2 = _repository.Create(new Cita { Matricula = "2222bbb", FechaInspeccion = DateTime.Today, Dni = "11111111Z" }).Value; 
        var c3 = _repository.Create(new Cita { Matricula = "5555ttt", FechaInspeccion = DateTime.Today, Dni = "11111111Z" }).Value;
        var c4 = _repository.Create(new Cita { Matricula = "4444ggg", FechaInspeccion = DateTime.Today, Dni = "22222222Z" }).Value;
            
        var actualizada = c4 with { Dni = "11111111Z" };

        // act
        var resultado = _repository.Update(c4.Id, actualizada);

        // assert
        resultado.IsFailure.Should().BeTrue();
        (resultado.Error as CitaError.Database)?.Detalles.Should()
            .Contain($"No se puede actualizar: El propietario con DNI {actualizada.Dni} ya tiene el límite de {AppConfig.MaxVehiculosPorDni} citas asignadas para ese día.");
    }
    
    [Test]
    public void Update_CambiandoDniSinexcederLimite_DeberiaDarSucc  () {
        // arrange
        var c1 = _repository.Create(new Cita { Matricula = "1111aaa", FechaInspeccion = DateTime.Today, Dni = "11111111Z" }).Value;
        var c2 = _repository.Create(new Cita { Matricula = "2222bbb", FechaInspeccion = DateTime.Today, Dni = "11111111Z" }).Value; 
        var c4 = _repository.Create(new Cita { Matricula = "4444ggg", FechaInspeccion = DateTime.Today, Dni = "22222222Z" }).Value;
            
        var actualizada = c4 with { Dni = "11111111Z" };

        // act
        var resultado = _repository.Update(c4.Id, actualizada);

        // assert
        resultado.IsFailure.Should().BeFalse();
    }
    
    [Test]
    public void Delete_BorradoLogico_DebeMarcarIsDeletedYColocarFecha() {
        // arrange
        _context.Citas.Add(new CitaEntity { Id = 10, Matricula = "4444ggg", Dni = "22222222Z" });
        _context.SaveChanges();

        // act
        var eliminado = _repository.Delete(10);

        // assert
        eliminado.Should().NotBeNull();
        var entidadDb = _context.Citas.First(c => c.Id == 10);
        entidadDb.IsDeleted.Should().BeTrue();
        entidadDb.DeletedAt.Should().NotBeNull();
    }
    
    [Test]
    public void Delete_CitaInexistente_DebeRetornarFailure() {
        // arrang y act
        var eliminado = _repository.Delete(10);

        // assert
        eliminado.Should().BeNull();
    }
    
    [Test]
    public void Delete_BorradoFisico_DebeEliminarTotalmente() {
        // arrange
        _context.Citas.Add(new CitaEntity { Id = 10, Matricula = "4444ggg", Dni = "22222222Z" });
        _context.SaveChanges();

        // act
        var eliminado = _repository.Delete(10, false);

        // assert
        _repository.GetById(10).Should().BeNull();
    }
    
    [Test]
    public void Restore_CitaExistenteYBorrada_DebeRestaurarlaCorrectamente() {
        // arrange
        _context.Citas.Add(new CitaEntity { Id = 5, Matricula = "4444ggg", Dni = "22222222Z", IsDeleted = true, DeletedAt = DateTime.Now });
        _context.SaveChanges();

        // act
        var resultado = _repository.Restore(5);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.IsDeleted.Should().BeFalse();
        _context.Citas.First(c => c.Id == 5).IsDeleted.Should().BeFalse();
    }
    
    [Test]
    public void Restore_CitaInexistente_DebeFallar() {
        // act
        var resultado = _repository.Restore(5);

        // assert
        resultado.IsSuccess.Should().BeFalse();
        (resultado.Error as CitaError.Database)?.Detalles.Should()
            .Contain($"No se puede restaurar una Cita que no existe.");
    }
    
    [Test]
    public void Restore_CitaMismaMatriculaParafecha_DebeFallar() {
        // arrange
        var citaExistente = _repository.Create(new Cita { Matricula = "1111aaa", FechaInspeccion = DateTime.Today, Dni = "11111111Z" , IsDeleted = true, DeletedAt = DateTime.UtcNow}).Value;
        _repository.Delete(citaExistente.Id, isLogical: true);
        var otraCita = _repository.Create(new Cita { Matricula = "1111aaa", FechaInspeccion = DateTime.Today, Dni = "11111111Z"}).Value;
            
        // act
        var resultado = _repository.Restore(citaExistente.Id);

        // assert
        resultado.IsSuccess.Should().BeFalse();
        (resultado.Error as CitaError.Database)?.Detalles.Should()
            .Contain($"No se puede restaurar: El vehículo ya cuenta con otra cita activa ese mismo día.");
    }
    
    [Test]
    public void Restore_CitaYaBorrada_DebeRestaurarlaCorrectamente() {
        // arrange
        _context.Citas.Add(new CitaEntity { Id = 5, Matricula = "4444ggg", Dni = "22222222Z", IsDeleted = false});
        _context.SaveChanges();

        // act
        var resultado = _repository.Restore(5);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.IsDeleted.Should().BeFalse();
        _context.Citas.First(c => c.Id == 5).IsDeleted.Should().BeFalse();
    }
    
    [Test]
    public void GetWithFilters_DebeFiltrarPorMotoresYSearchText() {
        // arrange
        _context.Citas.AddRange(new List<CitaEntity> {
            new() { Id = 1, Matricula = "AABB123", Dni = "11111111d", Motor = 0, FechaInspeccion = DateTime.Today, Marca = "Toyota", Modelo = "Yaris" }, 
            new() { Id = 2, Matricula = "CCDD456", Dni = "22222222s", Motor = 1, FechaInspeccion = DateTime.Today, Marca = "Renault", Modelo = "Clio" }, 
            new() { Id = 3, Matricula = "EEFF789", Dni = "33333333f", Motor = 2, FechaInspeccion = DateTime.Today, Marca = "Ford", Modelo = "Fiesta" },
            new() { Id = 4, Matricula = "1111ccc", Dni = "34444444c", Motor = 3, FechaInspeccion = DateTime.Today, Marca = "Skoda", Modelo = "Octavia" }
        });
        _context.SaveChanges();

        // act
        var resultadoG = _repository.GetWithFilters(
            fechaInicio: DateTime.Today.AddDays(-1),
            fechaFin: DateTime.Today.AddDays(1),
            pagina: 1,
            tamPagina: 10,
            searchText: "Toyota",
            motorSeleccionado: "gasolina",
            incluirEliminados: false
        );
        var resultadoD = _repository.GetWithFilters(
            fechaInicio: DateTime.Today.AddDays(-1),
            fechaFin: DateTime.Today.AddDays(1),
            pagina: 1,
            tamPagina: 10,
            searchText: "Renault",
            motorSeleccionado: "diesel",
            incluirEliminados: false
        );
        var resultadoE = _repository.GetWithFilters(
            fechaInicio: DateTime.Today.AddDays(-1),
            fechaFin: DateTime.Today.AddDays(1),
            pagina: 1,
            tamPagina: 10,
            searchText: "Ford",
            motorSeleccionado: "electrico",
            incluirEliminados: false
        );
        var resultadoH = _repository.GetWithFilters(
            fechaInicio: DateTime.Today.AddDays(-1),
            fechaFin: DateTime.Today.AddDays(1),
            pagina: 1,
            tamPagina: 10,
            searchText: "Skoda",
            motorSeleccionado: "hibrido",
            incluirEliminados: false
        );

        // assert
        resultadoG.IsSuccess.Should().BeTrue();
        resultadoG.Value.Should().HaveCount(1);
        resultadoG.Value.First().Matricula.Should().Be("AABB123");
        resultadoD.IsSuccess.Should().BeTrue();
        resultadoD.Value.Should().HaveCount(1);
        resultadoD.Value.First().Matricula.Should().Be("CCDD456");
        resultadoE.IsSuccess.Should().BeTrue();
        resultadoE.Value.Should().HaveCount(1);
        resultadoE.Value.First().Matricula.Should().Be("EEFF789");
        resultadoH.IsSuccess.Should().BeTrue();
        resultadoH.Value.Should().HaveCount(1);
        resultadoH.Value.First().Matricula.Should().Be("1111ccc");
    }
    
    [Test]
    public void GetWithFiltersMotoresTodos_DebeFiltrarPorMotoresYSearchText() {
        // arrange
        _context.Citas.AddRange(new List<CitaEntity> {
            new() { Id = 1, Matricula = "AABB123", Dni = "11111111d", Motor = 0, FechaInspeccion = DateTime.Today, Marca = "Toyota", Modelo = "Yaris" }, 
            new() { Id = 2, Matricula = "CCDD456", Dni = "22222222s", Motor = 1, FechaInspeccion = DateTime.Today, Marca = "Renault", Modelo = "Clio" }, 
            new() { Id = 3, Matricula = "EEFF789", Dni = "33333333f", Motor = 0, FechaInspeccion = DateTime.Today, Marca = "Ford", Modelo = "Fiesta" },
            new() { Id = 4, Matricula = "1111ccc", Dni = "34444444c", Motor = 0, FechaInspeccion = DateTime.Today, Marca = "Skoda", Modelo = "Octavia" }
        });
        _context.SaveChanges();

        // act
        var resultadoG = _repository.GetWithFilters(
            fechaInicio: DateTime.Today.AddDays(-1),
            fechaFin: DateTime.Today.AddDays(1),
            pagina: 1,
            tamPagina: 10,
            searchText: null,
            motorSeleccionado: "todos",
            incluirEliminados: false
        );

        // assert
        resultadoG.IsSuccess.Should().BeTrue();
        resultadoG.Value.Should().HaveCount(4);
        resultadoG.Value.First().Matricula.Should().Be("AABB123");
    }
    
    [Test]
    public void CountCitasFiltradas_DebeDarLaCantidadCorrectaDeItems() {
        // arrange
        _context.Citas.AddRange(new List<CitaEntity> {
            new() { Id = 1, Matricula = "AABB123", Dni = "11111111d", Motor = 0, FechaInspeccion = DateTime.Today, Marca = "Toyota", Modelo = "Yaris" }, 
            new() { Id = 2, Matricula = "CCDD456", Dni = "22222222s", Motor = 1, FechaInspeccion = DateTime.Today, Marca = "Renault", Modelo = "Clio" }, 
            new() { Id = 3, Matricula = "EEFF789", Dni = "33333333f", Motor = 2, FechaInspeccion = DateTime.Today, Marca = "Ford", Modelo = "Fiesta" },
            new() { Id = 4, Matricula = "1111ccc", Dni = "34444444c", Motor = 3, FechaInspeccion = DateTime.Today, Marca = "Skoda", Modelo = "Octavia" }
        });
        _context.SaveChanges();

        // act
        var conteoG = _repository.CountCitasFiltradas(
            searchText: null,
            fechaInicio: DateTime.Today.AddDays(-1),
            fechaFin: DateTime.Today.AddDays(1),
            incluirEliminados: false,
            motorSeleccionado: "gasolina"
        );
        var conteoD = _repository.CountCitasFiltradas(
            searchText: null,
            fechaInicio: DateTime.Today.AddDays(-1),
            fechaFin: DateTime.Today.AddDays(1),
            incluirEliminados: false,
            motorSeleccionado: "diesel"
        );
        var conteoE = _repository.CountCitasFiltradas(
            searchText: null,
            fechaInicio: DateTime.Today.AddDays(-1),
            fechaFin: DateTime.Today.AddDays(1),
            incluirEliminados: false,
            motorSeleccionado: "electrico"
        );
        var conteoH = _repository.CountCitasFiltradas(
            searchText: null,
            fechaInicio: DateTime.Today.AddDays(-1),
            fechaFin: DateTime.Today.AddDays(1),
            incluirEliminados: false,
            motorSeleccionado: "hibrido"
        );

        // assert
        conteoG.Should().Be(1);
        conteoD.Should().Be(1);
        conteoE.Should().Be(1);
        conteoH.Should().Be(1);
    }
    
    [Test]
    public void CountCitasFiltradasConSearchText_DebeDarLaCantidadCorrectaDeItems() {
        // arrange
        _context.Citas.AddRange(new List<CitaEntity> {
            new() { Id = 1, Matricula = "AABB123", Dni = "11111111d", Motor = 0, FechaInspeccion = DateTime.Today, Marca = "Toyota", Modelo = "Yaris" }, 
            new() { Id = 2, Matricula = "CCDD456", Dni = "22222222s", Motor = 1, FechaInspeccion = DateTime.Today, Marca = "Renault", Modelo = "Clio" }, 
            new() { Id = 3, Matricula = "EEFF789", Dni = "33333333f", Motor = 2, FechaInspeccion = DateTime.Today, Marca = "Ford", Modelo = "Fiesta" },
            new() { Id = 4, Matricula = "1111ccc", Dni = "34444444c", Motor = 3, FechaInspeccion = DateTime.Today, Marca = "Skoda", Modelo = "Octavia" }
        });
        _context.SaveChanges();

        // act
        var conteo = _repository.CountCitasFiltradas(
            searchText: "Toyota",
            fechaInicio: DateTime.Today.AddDays(-1),
            fechaFin: DateTime.Today.AddDays(1),
            incluirEliminados: false,
            motorSeleccionado: "todos"
        );

        // assert
        conteo.Should().Be(1);
    }
    
    [Test]
    public void Constructor_ConDropDataYSeedData_DeberiaInicializarYAgregarSemilla() {
        // act
        using var conn = _connection;
        conn.Open();
            
        var repoConSeed = new CitaEfCoreRepository(_context, true, true);

        // assert
        repoConSeed.CountCita( true).Should().BeGreaterThan(0);
    }
    
    [Test]
    public void GetByMatricula_CitaInexistente_DeberiaRetornarFailure() {
        // arrange y act
        var resultado = _repository.GetByMatricula("AABB123");

        // assert
        resultado.Should().BeNull();
    }
    
    [Test]
    public void GetByMatricula_CitaActiva_DeberiaRetornarla() {
        // arrange
        _repository.Create(new Cita { Id = 1, Matricula = "AABB123", Dni = "11111111d", Motor = 0, FechaInspeccion = DateTime.Today, Marca = "Toyota", Modelo = "Yaris" });

        // act
        var resultado = _repository.GetByMatricula("AABB123");

        // assert
        resultado.Should().NotBeNull();
        resultado.Matricula.Should().Be("AABB123");
    }
    
    [Test]
    public void DeleteAll_DeberiaVaciarLaTabla() {
        // arrange
        _repository.Create(new Cita { Id = 1, Matricula = "AABB123", Dni = "11111111d", Motor = 0, FechaInspeccion = DateTime.Today, Marca = "Toyota", Modelo = "Yaris" });

        // act
        var resultado = _repository.DeleteAll();

        // assert
        resultado.Should().BeTrue();
        _repository.CountCita(incluirEliminados: true).Should().Be(0);
        _repository.CountCita(incluirEliminados: false).Should().Be(0);
    }

}