using FluentAssertions;
using Manager.Models;
using Manager.Validators.Citas;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Manager.Test.Validators.Citas;

/// <summary>
/// Clase que almacena los test del validador.
/// Se encarga de asegurar que este valide correctamente todos los datos de la Cita.
/// Usa patrón AAA.
/// </summary>
[TestFixture]
public class CitaValidatorTest {

    
    /// <summary>
    /// Almacena validaciones a citas válidas
    /// </summary>
    [TestFixture]
    public class CasosCorrectos {
        
        // instanciacion del validador
        private CitaValidator _validador = null!;
        
        [SetUp]
        public void SetUp() {
            _validador = new CitaValidator();
        }

        // caso normal
        [Test]
        public void Validar_CitaValida_RetornaSucces() {
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111BBB",
                Dni = "12345678Z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = 2000,
                Motor = Cita.TiposMotor.Diesel,
                FechaMatriculacion = DateTime.Now.AddYears(-20),
                FechaInspeccion = DateTime.Now.AddDays(10),
            };

            // act
            var res = _validador.Validar(c);
            
            // assert
            res.IsSuccess.Should().BeTrue();
        }
        
        // casos correctos forzando varios ejemplos por campo, rozando valores límite
        [Test]
        [TestCase("1111BBB")]
        [TestCase("1111-BBB")]
        [TestCase("9999 BBB")]
        [TestCase("7777  nnn")]
        public void Validar_MatriculaValida_RetornaSucces(string matricula) {
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = matricula,
                Dni = "12345678Z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = 2000,
                Motor = Cita.TiposMotor.Diesel,
                FechaMatriculacion = DateTime.Now.AddYears(-20),
                FechaInspeccion = DateTime.Now.AddDays(10),
            };

            // act
            var res = _validador.Validar(c);
            
            // assert
            res.IsSuccess.Should().BeTrue();
        }
        
        
        
    }


    
    /// <summary>
    /// Almacena validaciones a citas inválidas
    /// </summary>
    [TestFixture]
    public class CasosIncorrectos {
        
        // instanciacion del validador
        private CitaValidator _validador = null!;
        
        [SetUp]
        public void SetUp() {
            _validador = new CitaValidator();
        }

        
        
        
        
    }
    
    
    
}