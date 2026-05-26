using System.IO.Compression;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Manager.Errors.Common;
using Manager.Errors.Storage;
using Manager.Models;
using Manager.Service.Backup;
using Manager.Storage.Common;
using Moq;

namespace Manager.Test.Service.Backup;

[TestFixture]
[TestOf(typeof(BackupService))]
public class BackupServiceTest {

    private string _tempDir = null!;
    private string _backupDir = null!;
    private Mock<IStorage<Cita>> _storageMock = null!;
    private BackupService _service = null!;

    [SetUp]
    public void SetUp() {
        _tempDir = Path.Combine(Path.GetTempPath(), $"BackupTest_{Guid.NewGuid()}");
        _backupDir = Path.Combine(_tempDir, "backups");
        Directory.CreateDirectory(_backupDir);

        _storageMock = new Mock<IStorage<Cita>>();
        _service = new BackupService(_storageMock.Object, _backupDir);
    }

    [TearDown]
    public void TearDown() {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
    }

    private static Cita CrearCita(string matricula = "1234BBC", string dni = "12345678Z") => new() {
        Id = 1,
        Matricula = matricula,
        Dni = dni,
        Marca = "Toyota",
        Modelo = "Corolla",
        Cilindrada = 1800,
        Motor = Cita.TiposMotor.Gasolina,
        FechaMatriculacion = DateTime.Today.AddYears(-3),
        FechaInspeccion = DateTime.Today.AddDays(10)
    };

    
    public class RealizarBackUpTest : BackupServiceTest {
        [Test]
        public void RealizarBackup_ConListaVacia_DeberiaRetornarFailure() {
            // act
            var resultado = _service.RealizarBackup([]);

            // assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<StorageError.WriteError>();
            resultado.Error.Mensaje.Should().Contain("No hay datos para respaldar");
        }

        [Test]
        public void RealizarBackup_ConCitas_DeberiaCrearZipEnDirectorio() {
            // arrange
            var citas = new List<Cita> { CrearCita() };
            _storageMock.Setup(s => s.WriteToFile(It.IsAny<IEnumerable<Cita>>(), It.IsAny<string>()))
                .Returns(Result.Success<bool, DomainError>(true));

            // act
            var resultado = _service.RealizarBackup(citas);

            // assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().EndWith(".zip");
            File.Exists(resultado.Value).Should().BeTrue();
            _storageMock.Verify(s => s.WriteToFile(citas, It.Is<string>(p => p.EndsWith("citas.json"))), Times.Once);
        }

        [Test]
        public void RealizarBackup_ConDirectorioCustom_DeberiaCrearEnEseDirectorio() {
            // arrange
            var customDir = Path.Combine(_tempDir, "custom");
            Directory.CreateDirectory(customDir);
            var citas = new List<Cita> { CrearCita() };
            _storageMock.Setup(s => s.WriteToFile(It.IsAny<IEnumerable<Cita>>(), It.IsAny<string>()))
                .Returns(Result.Success<bool, DomainError>(true));

            // act
            var resultado = _service.RealizarBackup(citas, customDir);

            // assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().StartWith(customDir);
            File.Exists(resultado.Value).Should().BeTrue();
        }

        [Test]
        public void RealizarBackup_CuandoStorageFalla_DeberiaRetornarFailure() {
            // arrange
            var citas = new List<Cita> { CrearCita() };
            _storageMock.Setup(s => s.WriteToFile(It.IsAny<IEnumerable<Cita>>(), It.IsAny<string>()))
                .Returns(Result.Failure<bool, DomainError>(StorageErrors.WriteError("Fallo simulado")));

            // act
            var resultado = _service.RealizarBackup(citas, _backupDir);

            // assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<StorageError.WriteError>();
        }
    }
    
    public class TestRestaurarBackUp: BackupServiceTest {
        
        [Test]
        public void RestaurarBackup_ConArchivoInexistente_DeberiaRetornarFailure() {
            // act
            var resultado = _service.RestaurarBackup(Path.Combine(_tempDir, "noexiste.zip"));

            // assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<StorageError.ArchivoNoEncontrado>();
        }

        [Test]
        public void RestaurarBackup_ConZipValido_DeberiaRetornarCitas() {
            // arrange
            var citas = new List<Cita> { CrearCita() };
            _storageMock.Setup(s => s.ReadFromFile(It.IsAny<string>()))
                .Returns(Result.Success<IEnumerable<Cita>, DomainError>(citas));

            var zipPath = Path.Combine(_backupDir, "test-backup.zip");
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
                zip.CreateEntry("data/citas.json");
            }

            // act
            var resultado = _service.RestaurarBackup(zipPath);

            // assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().HaveCount(1);
        }

        [Test]
        public void RestaurarBackup_ConZipSinCitasJson_DeberiaRetornarFailure() {
            // arrange
            var zipPath = Path.Combine(_backupDir, "sin-data.zip");
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
                zip.CreateEntry("otro.txt");
            }

            // act
            var resultado = _service.RestaurarBackup(zipPath);

            // assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<StorageError.ArchivoNoEncontrado>();
        }

        [Test]
        public void RestaurarBackup_ConZipCorrupto_DeberiaRetornarFailure() {
            // arrange
            var zipPath = Path.Combine(_backupDir, "corrupto.zip");
            File.WriteAllText(zipPath, "esto no es un zip");

            // act
            var resultado = _service.RestaurarBackup(zipPath);

            // assert
            resultado.IsFailure.Should().BeTrue();
        }
    }


    public class ListarbackupsTest : BackupServiceTest {
        [Test]
        public void ListarBackups_SinBackups_DeberiaRetornarVacio() {
            // act
            var resultado = _service.ListarBackups();

            // assert
            resultado.Should().BeEmpty();
        }

        [Test]
        public void ListarBackups_ConUnBackupCreado_DeberiaRetornarloEnLista() {
            // arrange
            var citas = new List<Cita> { CrearCita() };
            _storageMock.Setup(s => s.WriteToFile(It.IsAny<IEnumerable<Cita>>(), It.IsAny<string>()))
                .Returns(Result.Success<bool, DomainError>(true));
            var backup = _service.RealizarBackup(citas);
            backup.IsSuccess.Should().BeTrue();

            // act
            var resultado = _service.ListarBackups().ToList();

            // assert
            resultado.Should().HaveCount(1);
            resultado[0].Should().Be(backup.Value);
        }

        [Test]
        public void ListarBackups_ConDirectorioCustom_DeberiaBuscarEnEseDirectorio() {
            // arrange
            var customDir = Path.Combine(_tempDir, "custom-list");
            Directory.CreateDirectory(customDir);
            var citas = new List<Cita> { CrearCita() };
            _storageMock.Setup(s => s.WriteToFile(It.IsAny<IEnumerable<Cita>>(), It.IsAny<string>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _service.RealizarBackup(citas, customDir);

            // act
            var resultadoDefault = _service.ListarBackups().ToList();
            var resultadoCustom = _service.ListarBackups(customDir).ToList();

            // assert
            resultadoDefault.Should().BeEmpty();
            resultadoCustom.Should().HaveCount(1);
        }

        [Test]
        public void ListarBackups_ConDirectorioInexistente_DeberiaRetornarVacio() {
            // act
            var resultado = _service.ListarBackups(Path.Combine(_tempDir, "no-existe"));

            // assert
            resultado.Should().BeEmpty();
        }
    }
}