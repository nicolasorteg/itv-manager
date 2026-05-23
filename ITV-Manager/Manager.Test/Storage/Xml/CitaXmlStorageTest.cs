using FluentAssertions;
using Manager.Config;
using Manager.Errors.Storage;
using Manager.Models;
using Manager.Storage.Xml;

namespace Manager.Test.Storage.Xml;

[TestFixture] 
[TestOf(typeof(CitaXmlStorage))]
public class CitaXmlStorageTest {
    private CitaXmlStorage _storage;
    private string _tempPath;
    
    [SetUp]
    public void SetUp() {
        _storage = new CitaXmlStorage();
        _tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xml");
    }
    
    [TearDown]
    public void TearDown() {
        if (File.Exists(_tempPath)) File.Delete(_tempPath); // borra el temp xml
    }

    public class CasosValidos : CitaXmlStorageTest {
         
        [Test]
        public void WriteToFiles_DeberiaGuardarCorrectamente() {
            // arrange
            var citas = new List<Cita> {
                new Cita { Matricula = "1111bbb", Marca = "Skoda", Modelo = "Octavia", Cilindrada = 1800, Dni = "56478930f", Motor = Cita.TiposMotor.Diesel},
                new Cita { Matricula = "2222ccc", Marca = "Citroen", Modelo = "C4" , Cilindrada = 2000, Dni = "12345678z", Motor = Cita.TiposMotor.Gasolina}
            };

            // act
            var res = _storage.WriteToFile(citas, _tempPath);

            // assert
            res.IsSuccess.Should().BeTrue();
            File.Exists(_tempPath).Should().BeTrue();
        }
        
        [Test]
        public void ReadFromFile_DeberiaRetornarDatos() {
            // arrange
            var citas = new List<Cita> {
                new Cita { Matricula = "1111bbb", Marca = "Skoda", Modelo = "Octavia", Cilindrada = 1800, Dni = "56478930f", Motor = Cita.TiposMotor.Diesel},
                new Cita { Matricula = "2222ccc", Marca = "Citroen", Modelo = "C4" , Cilindrada = 2000, Dni = "12345678z", Motor = Cita.TiposMotor.Gasolina}
            };
            _storage.WriteToFile(citas, _tempPath);

            // act
            var res = _storage.ReadFromFile(_tempPath);

            // assert
            res.IsSuccess.Should().BeTrue();
            res.Value.Should().HaveCount(2);
            res.Value.First().Matricula.Should().Be("1111bbb");
            res.Value.First().Should().BeOfType<Cita>();
        }
        
        [Test]
        public void WriteToFile_ListaVacia_DeberiaCrearArchivoVacio() {
            // arrange y act
            var resultado = _storage.WriteToFile(new List<Cita>(), _tempPath);

            // assert
            resultado.IsSuccess.Should().BeTrue();
            File.Exists(_tempPath).Should().BeTrue();
        }
    }
    
    public class CasosInvalidos : CitaXmlStorageTest {
       
        [Test]
        public void Cargar_CuandoArchivoNoExiste_DeberiaRetornarError() {
            // arrange y act
            var resultado = _storage.ReadFromFile("dhsgakdysgad.xml");

            // asert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<StorageError.ArchivoNoEncontrado>();
            (resultado.Error as StorageError.ArchivoNoEncontrado)?.Path.Should().Be("dhsgakdysgad.xml");
            resultado.Error.Mensaje.Should().Contain("dhsgakdysgad.xml");
        }
        
        [Test]
        public void ReadFromFile_CuandoXmlEstaCorrupto_DebeLanzarExcepcionYEntrarEnCatch() {
            // arrange
            File.WriteAllText(_tempPath, "TextoCorruptodksdnas");

            // act
            var resultado = _storage.ReadFromFile(_tempPath);

            // assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<StorageError.AccesoError>();
        }
        
        [Test]
        public void WriteToFile_RutaInvalidaOInaccesible_DebeLanzarExcepcionYEntrarEnCatch() {
            // arrange
            var listaCitas = new List<Cita> { new Cita { Matricula = "1111bbb" } };

            // act
            var resultado = _storage.WriteToFile(listaCitas, "");

            // assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<StorageError.WriteError>(); 
        }
    }
}