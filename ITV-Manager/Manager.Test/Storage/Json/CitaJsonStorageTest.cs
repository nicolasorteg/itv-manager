using System.Text;
using FluentAssertions;
using Manager.Errors.Storage;
using Manager.Models;
using Manager.Storage.Json;

namespace Manager.Test.Storage.Json;

[TestFixture] 
[TestOf(typeof(CitaJsonStorage))]
public class CitaJsonStorageTest {
    
    private CitaJsonStorage _storage;
    private string _tempPath;
    
    [SetUp]
    public void SetUp() {
        _storage = new CitaJsonStorage();
        _tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
    }
    
    [TearDown]
    public void TearDown() {
        if (File.Exists(_tempPath)) File.Delete(_tempPath); // borra el temp json
    }


    [TestFixture] 
    public class CasosPositivos : CitaJsonStorageTest {
        
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
            var resultado = _storage.WriteToFile(citas, _tempPath);

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
            _storage.WriteToFile(citas, _tempPath);

            // act
            var resultado = _storage.ReadFromFile(_tempPath);

            // assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().HaveCount(2);

            var citaRecuperada = resultado.Value.First();
            citaRecuperada.Matricula.Should().Be("1111bbb");
            citaRecuperada.Cilindrada.Should().Be(2000);
            citaRecuperada.Motor.Should().Be(Cita.TiposMotor.Diesel);
        }
    }
    
    [TestFixture] 
    public class CasosNegativos : CitaJsonStorageTest {
        
        [Test]
        public void Save_ArchivoInexistente_DeberiaRetornarErrorArchivoNoEncontrado() {

            // arrange y act
            var resultado = _storage.ReadFromFile("ruta/completamente/inexistente.csv");

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
            var resultado = _storage.ReadFromFile(_tempPath);

            // assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<StorageError.FormatoInvalido>();
        }

        [Test]
        public void Write_EnRutaInvalida_DeberiaRetornarWriteError() {

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
            const string RutaInvalida = "/rutainvaluida/archivo.json";


            _storage.WriteToFile(citas, RutaInvalida);

            // act
            var resultado = _storage.WriteToFile(citas, RutaInvalida);

            // assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<StorageError.WriteError>();
            resultado.Error.Mensaje.Should().Contain("Error al escribir en el almacenamiento.");
        }
        
        [Test]
        public void Save_ArchivoJsonContienePalabraNull_DeberiaEntrarEnIfDeDtosNulosYRetornarFormatoInvalido() {
            
            using (var writer = new StreamWriter(_tempPath, false, Encoding.UTF8)) {
                writer.WriteLine("null");
            }

            // act
            var resultado = _storage.ReadFromFile(_tempPath);

            // assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<StorageError.FormatoInvalido>(); 
            resultado.Error.Mensaje.Should().Contain("No se pudieron deserializar los DTO de Citas.");
        }
        
        [Test]
        public void Constructor_ConCarpetaInexistente_DeberiaCrearDirectorio() {

            // arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            // act
            var storage = new CitaJsonStorage(tempDir);

            // assert
            Directory.Exists(tempDir).Should().BeTrue();
            Directory.Delete(tempDir);
        }
    }
}