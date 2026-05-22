using System.Text;
using FluentAssertions;
using Manager.Errors.Storage;
using Manager.Models;
using Manager.Storage.Csv;

namespace Manager.Test.Storage.Csv;

[TestFixture]
[TestOf(typeof(CitaCsvStorage))]
public class CitaCsvStorageTest {
    
    private CitaCsvStorage _storage;
    private string _tempPath;
    
    [SetUp]
    public void SetUp() {
        _storage = new CitaCsvStorage();
        _tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
    }
    
    [TearDown]
    public void TearDown() {
        if (File.Exists(_tempPath)) File.Delete(_tempPath); // borra el temp csv
    }


    [TestFixture] public class CasosPositivos : CitaCsvStorageTest {

        [Test]
        public void Write_ConDatosValidos_DeberiaRetornarSuccess() {

            // arrange
            var citas = new List<Cita> {
                new() {
                    Id = 1,
                    Matricula = "1111bbb",
                    Dni = "12345678Z",
                    Marca = "Skoda",
                    Modelo = "Octavia",
                    Cilindrada = 2000,
                    Motor = Cita.TiposMotor.Diesel,
                    FechaMatriculacion = DateTime.Now.AddYears(-20),
                    FechaInspeccion = DateTime.Now.AddDays(10),
                },
                new() {
                    Id = 2,
                    Matricula = "3333bbb",
                    Dni = "12345678Z",
                    Marca = "Citroen",
                    Modelo = "C4",
                    Cilindrada = 1500,
                    Motor = Cita.TiposMotor.Gasolina,
                    FechaMatriculacion = DateTime.Now.AddYears(-10),
                    FechaInspeccion = DateTime.Now.AddDays(20),
                }
            };

            // act
            var resultado = _storage.Write(citas, _tempPath);

            // assert
            resultado.IsSuccess.Should().BeTrue();
            File.Exists(_tempPath).Should().BeTrue();
        }

        [Test]
        public void Save_ConArchivoExistente_DeberiaRetornarSuccess() {

            // arrange
            var citas = new List<Cita> {
                new() {
                    Id = 1,
                    Matricula = "1111bbb",
                    Dni = "12345678Z",
                    Marca = "Skoda",
                    Modelo = "Octavia",
                    Cilindrada = 2000,
                    Motor = Cita.TiposMotor.Diesel,
                    FechaMatriculacion = DateTime.Now.AddYears(-20),
                    FechaInspeccion = DateTime.Now.AddDays(10),
                },
                new() {
                    Id = 2,
                    Matricula = "3333bbb",
                    Dni = "12345678Z",
                    Marca = "Citroen",
                    Modelo = "C4",
                    Cilindrada = 1500,
                    Motor = Cita.TiposMotor.Gasolina,
                    FechaMatriculacion = DateTime.Now.AddYears(-10),
                    FechaInspeccion = DateTime.Now.AddDays(20),
                }
            };
            _storage.Write(citas, _tempPath);

            // act
            var resultado = _storage.Save(_tempPath);

            // assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().HaveCount(2);

            var citaRecuperada = resultado.Value.First();
            citaRecuperada.Matricula.Should().Be("1111bbb");
            citaRecuperada.Cilindrada.Should().Be(2000);
            citaRecuperada.Motor.Should().Be(Cita.TiposMotor.Diesel);
        }

        [Test]
        public void Write_ConCaracteresEspeciales_DeberiaEscaparCorrectamente() {

            // arrange
            var citas = new List<Cita> {
                new() {
                    Id = 1,
                    Matricula = "1111;BBB",
                    Dni = "123\"456",
                    Marca = "Skoda\nLinea",
                    Modelo = "Octavia\rModelo",
                    Cilindrada = 2000,
                    Motor = Cita.TiposMotor.Diesel,
                    FechaMatriculacion = DateTime.UtcNow,
                    FechaInspeccion = DateTime.UtcNow
                }
            };

            // act
            var result = _storage.Write(citas, _tempPath);

            // assert
            result.IsSuccess.Should().BeTrue();

            var contenido = File.ReadAllText(_tempPath);

            contenido.Should().Contain("\"1111;BBB\"");
            contenido.Should().Contain("\"123\"\"456\"");
            contenido.Should().Contain("\"Skoda\nLinea\"");
        }
        
        [Test]
        public void Constructor_ConCarpetaInexistente_DeberiaCrearDirectorio() {

            // arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            // act
            var storage = new CitaCsvStorage(tempDir);

            // assert
            Directory.Exists(tempDir).Should().BeTrue();
            Directory.Delete(tempDir);
        }
    }

    [TestFixture] public class CasosNegativos : CitaCsvStorageTest {

        [Test]
        public void Save_ArchivoInexistente_DeberiaRetornarErrorArchivoNoEncontrado() {

            // arrange y act
            var resultado = _storage.Save("ruta/completamente/inexistente.csv");

            // assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<StorageError.ArchivoNoEncontrado>();
            resultado.Error.Mensaje.Should().Contain("No se ha encontrado el archivo en la ruta:");
        }

        [Test]
        public void Save_FormatoDelArchivoCorrupto_DeberiaRetornarFormatoInvalido() {

            // arrange
            using (var writer = new StreamWriter(_tempPath, false, Encoding.UTF8)) {
                writer.WriteLine(
                    "Id;Matricula;Dni;Marca;Modelo;Cilindrada;Motor;FechaMatriculacion;FechaInspeccion;CreatedAt;UpdatedAt;IsDeleted;DeletedAt");
                writer.WriteLine(
                    "IdInvalido;1234BBB;11111111A;BMW;M4;CilindradaInvalida;Diesel;2020-01-01;2026-05-21;2026-05-21;2026-05-21;false;");
            }

            // act
            var resultado = _storage.Save(_tempPath);

            // assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<StorageError.FormatoInvalido>();
        }

        [Test]
        public void Write_EnRutaInvalidaOInaccesible_DeberiaRetornarWriteError() {

            // arrange
            var citas = new List<Cita> {
                new() {
                    Id = 1,
                    Matricula = "1111bbb",
                    Dni = "12345678Z",
                    Marca = "Skoda",
                    Modelo = "Octavia",
                    Cilindrada = 2000,
                    Motor = Cita.TiposMotor.Diesel,
                    FechaMatriculacion = DateTime.Now.AddYears(-20),
                    FechaInspeccion = DateTime.Now.AddDays(10),
                },
                new() {
                    Id = 2,
                    Matricula = "3333bbb",
                    Dni = "12345678Z",
                    Marca = "Citroen",
                    Modelo = "C4",
                    Cilindrada = 1500,
                    Motor = Cita.TiposMotor.Gasolina,
                    FechaMatriculacion = DateTime.Now.AddYears(-10),
                    FechaInspeccion = DateTime.Now.AddDays(20),
                }
            };
            const string RutaInvalida = "/rutainvaluida/archivo.csv";


            _storage.Write(citas, RutaInvalida);

            // act
            var resultado = _storage.Write(citas, RutaInvalida);

            // assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<StorageError.WriteError>();
            resultado.Error.Mensaje.Should().Contain("Error al escribir en el almacenamiento.");
        }

    }

    [TestFixture]
    public class CasosMixtos : CitaCsvStorageTest {
        
        [Test]
        public void Write_ListaVacia_DeberiaCrearArchivoSoloConCabecera() {
            // arrange
            var citasVacias = new List<Cita>();

            // act
            var resultado = _storage.Write(citasVacias, _tempPath);

            // assert
            resultado.IsSuccess.Should().BeTrue();
            File.Exists(_tempPath).Should().BeTrue();
                
            var lineas = File.ReadAllLines(_tempPath);
            lineas.Should().HaveCount(1);
            lineas[0].Should().StartWith("Id;Matricula");
        }
    }

}