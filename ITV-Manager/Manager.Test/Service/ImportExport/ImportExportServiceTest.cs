using CSharpFunctionalExtensions;
using FluentAssertions;
using Manager.Errors.Common;
using Manager.Models;
using Manager.Service.ImportExport;
using Manager.Storage.Common;
using Moq;

namespace Manager.Test.Service.ImportExport;

[TestFixture]
[TestOf(typeof(ImportExportService))]
public class ImportExportServiceTest {
    
    // setup
    private string _tempDir;
    private ImportExportService _service;
    private Mock<IStorage<Cita>> _storageMock;
    
    [SetUp]
    public void SetUp() {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ImportExportTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        _storageMock = new Mock<IStorage<Cita>>();
        _service = new ImportExportService(_storageMock.Object);
    }

    [TearDown]
    public void TearDown() {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
    }
    
        
    [Test]
    public void ExportarDatos_ConCitas_DeberiaRetornarContador() {
        // arrange
        var citas = new List<Cita> {
            new() { Id = 1, Dni = "11111111A", Matricula = "1111ccc"},
            new() { Id = 2, Dni = "22222222B", Matricula = "2222ddd" },
            new() { Id = 3, Dni = "33333333C", Matricula = "3333fff" }
        };

        var path = Path.Combine(_tempDir, "data-citas.csv");
        var export = _storageMock.Setup(s => s.WriteToFile(citas, path))
            .Returns(Result.Success<bool, DomainError>(true));

        // act
        var resultado = _service.ExportarDatos(citas, path);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(3);
    }

    [Test]
    public void ImportarDatos_ConArchivo_DeberiaRetornarPersonas() {
        // arrange
        var citas = new List<Cita> {
            new() { Id = 1, Dni = "11111111A", Matricula = "1111ccc"},
            new() { Id = 2, Dni = "22222222B", Matricula = "2222ddd" },
            new() { Id = 3, Dni = "33333333C", Matricula = "3333fff" }
        };
        var path = Path.Combine(_tempDir, "data-citas.csv");

        _storageMock.Setup(s => s.ReadFromFile(path))
            .Returns(Result.Success<IEnumerable<Cita>, DomainError>(citas));

        // act
        var resultado = _service.ImportarDatos(path);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().HaveCount(3);
        resultado.Value.First().Dni.Should().Be("11111111A");
    }

    [Test]
    public void ExportarDatosSistema_DeberiaLLamarExportarDatosConRutaVacia() {
        // arrange
        var citas = new List<Cita> {
            new() { Id = 1, Dni = "11111111A", Matricula = "1111ccc"},
            new() { Id = 2, Dni = "22222222B", Matricula = "2222ddd" },
            new() { Id = 3, Dni = "33333333C", Matricula = "3333fff" }
        };
        _storageMock.Setup(s => s.WriteToFile(citas, It.IsAny<string>()))
            .Returns(Result.Success<bool, DomainError>(true));

        // act
        var res = _service.ExportarDatosSistema(citas);

        // assert
        res.IsSuccess.Should().BeTrue();
        _storageMock.Verify(s => s.WriteToFile(It.IsAny<IEnumerable<Cita>>(), string.Empty), Times.Once);
    }

    [Test]
    public void ImportarDatosSistema_ConRuta_DeberiaLLamarImportarDatos() {
        // arrange
        var citas = new List<Cita> {
            new() { Id = 1, Dni = "11111111A", Matricula = "1111ccc"},
            new() { Id = 2, Dni = "22222222B", Matricula = "2222ddd" },
            new() { Id = 3, Dni = "33333333C", Matricula = "3333fff" }
        };
        var path = Path.Combine(_tempDir, "test.json");
        

        _storageMock.Setup(s => s.ReadFromFile(path))
            .Returns(Result.Success<IEnumerable<Cita>, DomainError>(citas));

        // act
        var resultado = _service.ImportarDatosSistema(path);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        _storageMock.Verify(s => s.ReadFromFile(path), Times.Once);
    }

    [Test]
    public void ExportarDatos_ConListaVacia_DeberiaRetornarCero() {
        // arrange
        var citas = new List<Cita>();
        var path = Path.Combine(_tempDir, "data-vacia.json");

        _storageMock.Setup(s => s.WriteToFile(citas, path))
            .Returns(Result.Success<bool, DomainError>(true));

        // act
        var resultado = _service.ExportarDatos(citas, path);

        // assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(0);
    }
}