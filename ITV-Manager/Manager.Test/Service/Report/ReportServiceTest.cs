using FluentAssertions;
using Manager.Models;
using Manager.Service.Report;

namespace Manager.Test.Service.Report;

public class ReportServiceTest {
    private ReportService _reportService;
    private Cita _citaPrueba;
    private readonly List<string> _archivosGenerados = [];

    [SetUp]
    public void Setup() {
        _reportService = new ReportService();

        _citaPrueba = new Cita {
            Id = 1,
            Matricula = "1234ABC",
            Dni = "12345678Z",
            Marca = "Toyota",
            Modelo = "Yaris Cross",
            Motor = Cita.TiposMotor.Hibrido,
            FechaMatriculacion = new DateTime(2022, 05, 10),
            FechaInspeccion = DateTime.Today.AddDays(5)
        };
    }
    
    [TearDown]
    public void TearDown() {
        foreach (var archivo in _archivosGenerados.Where(archivo => File.Exists(archivo))) {
            try {
                File.Delete(archivo);
            } catch { }
        }

        _archivosGenerados.Clear();
    }

    [TestFixture] public class GeneracionDeDocumentos : ReportServiceTest {

        [Test]
        public void ExportarCitaAHtml_DeberiaCrearArchivoValidoYRetornarRuta() {
            // act
            var resultado = _reportService.ExportarCitaAHtml(_citaPrueba);

            // assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().NotBeNullOrEmpty();
            resultado.Value.Should().EndWith(".html");
            
            File.Exists(resultado.Value).Should().BeTrue();
            
            _archivosGenerados.Add(resultado.Value);
            
            var contenido = File.ReadAllText(resultado.Value);
            contenido.Should().Contain(_citaPrueba.Matricula);
            contenido.Should().Contain(_citaPrueba.Marca);
            contenido.Should().Contain(_citaPrueba.Dni);
        }
        
        [Test]
        public void ExportarCitaAPdf_DeberiaGenerarUnDocumentoPdfFisico() {
            // act
            var resultado = _reportService.ExportarCitaAPdf(_citaPrueba);

            // assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().NotBeNullOrEmpty();
            resultado.Value.Should().EndWith(".pdf");
            
            File.Exists(resultado.Value).Should().BeTrue();
            
            _archivosGenerados.Add(resultado.Value);
            
            var bytes = File.ReadAllBytes(resultado.Value);
            bytes.Length.Should().BeGreaterThan(0);
            System.Text.Encoding.UTF8.GetString(bytes[0..4]).Should().Be("%PDF");
        }
    }
}