using FluentAssertions;
using Manager.Errors.Citas;
using Manager.Models;
using Manager.Validators.Citas;

namespace Manager.Test.Validators.Citas;

/// <summary>
/// Clase que almacena los test del validador.
/// Se encarga de asegurar que este valide correctamente todos los datos de la Cita.
/// Usa patrón AAA.
/// </summary>
[TestFixture]
[TestOf(typeof(CitaValidator))]
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
        public void Validar_CitaValida_RetornaSuccess() {
            
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
        [TestCase("7777   nnn")]
        public void Validar_MatriculaValida_RetornaSuccess(string matricula) {
            
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
        
        [Test]
        [TestCase("12345678Z")]
        [TestCase("54836605       m")]
        public void Validar_DniValido_RetornaSuccess(string dni) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111bbb",
                Dni = dni,
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
        
        [Test]
        [TestCase("Skoda                      ")]
        [TestCase("S")]
        public void Validar_MarcaValida_RetornaSuccess(string marca) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111bbb",
                Dni = "12345678Z",
                Marca = marca,
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
        
        [Test]
        [TestCase("Octavia                      ")]
        [TestCase("O")]
        public void Validar_ModeloValido_RetornaSuccess(string modelo) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111bbb",
                Dni = "12345678Z",
                Marca = "Skoda",
                Modelo = modelo,
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
        
        [Test]
        [TestCase(1)]
        [TestCase(9000)]
        public void Validar_ModeloValido_RetornaSuccess(int cilindrada) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111bbb",
                Dni = "12345678Z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = cilindrada,
                Motor = Cita.TiposMotor.Diesel,
                FechaMatriculacion = DateTime.Now.AddYears(-20),
                FechaInspeccion = DateTime.Now.AddDays(10),
            };

            // act
            var res = _validador.Validar(c);
            
            // assert
            res.IsSuccess.Should().BeTrue();
        }
        
        [Test]
        [TestCase(Cita.TiposMotor.Diesel)]
        [TestCase(Cita.TiposMotor.Electrico)]
        [TestCase(Cita.TiposMotor.Gasolina)]
        [TestCase(Cita.TiposMotor.Hibrido)]
        public void Validar_ModeloValido_RetornaSuccess(Cita.TiposMotor motor) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111bbb",
                Dni = "12345678Z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = 2000,
                Motor = motor,
                FechaMatriculacion = DateTime.Now.AddYears(-20),
                FechaInspeccion = DateTime.Now.AddDays(10),
            };

            // act
            var res = _validador.Validar(c);
            
            // assert
            res.IsSuccess.Should().BeTrue();
        }
        
        [Test]
        [TestCase("2005-10-05")] // Y-M-D
        [TestCase("2026-05-13")]
        public void Validar_FechaMatriculacionValida_RetornaSuccess(DateTime fechaMatriculacion) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111bbb",
                Dni = "12345678Z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = 2000,
                Motor = Cita.TiposMotor.Diesel,
                FechaMatriculacion = fechaMatriculacion,
                FechaInspeccion = DateTime.Now.AddDays(10),
            };

            // act
            var res = _validador.Validar(c);
            
            // assert
            res.IsSuccess.Should().BeTrue();
        }
        
        [Test]
        [TestCase(30)] // Y-M-D
        [TestCase(0)] //
        public void Validar_FechaInspeccionValida_RetornaSuccess(int fechaInspeccion) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111bbb",
                Dni = "12345678Z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = 2000,
                Motor = Cita.TiposMotor.Diesel,
                FechaMatriculacion = DateTime.Now.AddYears(-20),
                FechaInspeccion = DateTime.Now.AddDays(fechaInspeccion),
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

        
        [Test]
        [TestCase("")]
        [TestCase("MatriculaInvalida")]
        [TestCase("BBBBBBB")]
        [TestCase("1111111")]
        [TestCase("1111AAA")]
        public void Validar_MatriculaInalida_RetornaFailure(string matricula) {
            
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
            res.IsSuccess.Should().BeFalse();
            
            res.Error.Should().BeOfType<CitaError.Validation>();
            
            (res.Error as CitaError.Validation)?.Errores.Should()
                .Contain("La matrícula no cumple el formato (NNNNLLL).");
        }
        
        [Test]
        [TestCase("")]
        [TestCase("DniInvalido")]
        [TestCase("aaaaaaaaa")]
        [TestCase("111111111")]
        [TestCase("12345678a")]
        public void Validar_DniInvalido_RetornaFailure(string dni) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111aaa",
                Dni = dni,
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
            res.IsSuccess.Should().BeFalse();
            
            res.Error.Should().BeOfType<CitaError.Validation>();
            
            (res.Error as CitaError.Validation)?.Errores.Should()
                .Contain("El DNI no es válido o la letra de control es incorrecta.");
        }
        
        [Test]
        [TestCase("")]
        [TestCase("MarcaDemasiadoL")]
        public void Validar_MarcaInvalida_RetornaFailure(string marca) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111aaa",
                Dni = "12345678z",
                Marca = marca,
                Modelo = "Octavia",
                Cilindrada = 2000,
                Motor = Cita.TiposMotor.Diesel,
                FechaMatriculacion = DateTime.Now.AddYears(-20),
                FechaInspeccion = DateTime.Now.AddDays(10),
            };

            // act
            var res = _validador.Validar(c);
            
            // assert
            res.IsSuccess.Should().BeFalse();
            
            res.Error.Should().BeOfType<CitaError.Validation>();
            
            (res.Error as CitaError.Validation)?.Errores.Should()
                .Contain("La marca es obligatoria y debe tener menos de 15 caracteres.");
        }
        
        [Test]
        [TestCase("")]
        [TestCase("ModeloDemasiado")]
        public void Validar_ModeloInvalido_RetornaFailure(string modelo) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111aaa",
                Dni = "12345678z",
                Marca = "Skoda",
                Modelo = modelo,
                Cilindrada = 2000,
                Motor = Cita.TiposMotor.Diesel,
                FechaMatriculacion = DateTime.Now.AddYears(-20),
                FechaInspeccion = DateTime.Now.AddDays(10),
            };

            // act
            var res = _validador.Validar(c);
            
            // assert
            res.IsSuccess.Should().BeFalse();
            
            res.Error.Should().BeOfType<CitaError.Validation>();
            
            (res.Error as CitaError.Validation)?.Errores.Should()
                .Contain("El modelo es obligatorio y debe tener menos de 15 caracteres.");
        }
        
        [Test]
        [TestCase(0)]
        [TestCase(9001)]
        public void Validar_CilindradaInvalida_RetornaFailure(int cilindrada) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111aaa",
                Dni = "12345678z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = cilindrada,
                Motor = Cita.TiposMotor.Diesel,
                FechaMatriculacion = DateTime.Now.AddYears(-20),
                FechaInspeccion = DateTime.Now.AddDays(10),
            };

            // act
            var res = _validador.Validar(c);
            
            // assert
            res.IsSuccess.Should().BeFalse();
            
            res.Error.Should().BeOfType<CitaError.Validation>();
            
            (res.Error as CitaError.Validation)?.Errores.Should()
                .Contain("La cilindrada debe estar entre 1 y 9000 cc.");
        }
        
        [Test]
        [TestCase((Cita.TiposMotor)4)]
        public void Validar_MotorInvalido_RetornaFailure(Cita.TiposMotor motor) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111aaa",
                Dni = "12345678z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = 2000,
                Motor = motor,
                FechaMatriculacion = DateTime.Now.AddYears(-20),
                FechaInspeccion = DateTime.Now.AddDays(10),
            };

            // act
            var res = _validador.Validar(c);
            
            // assert
            res.IsSuccess.Should().BeFalse();
            
            res.Error.Should().BeOfType<CitaError.Validation>();
            
            (res.Error as CitaError.Validation)?.Errores.Should()
                .Contain("El tipo de motor seleccionado no es válido para el sistema.");
        }
        
        [Test]
        [TestCase("2027-05-13")]
        public void Validar_FechaMtriculacionInvalida_RetornaFailure(DateTime fechaMatriculacion) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111aaa",
                Dni = "12345678z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = 2000,
                Motor = Cita.TiposMotor.Diesel,
                FechaMatriculacion = fechaMatriculacion,
                FechaInspeccion = DateTime.Now.AddDays(10),
            };

            // act
            var res = _validador.Validar(c);
            
            // assert
            res.IsSuccess.Should().BeFalse();
            
            res.Error.Should().BeOfType<CitaError.Validation>();
            
            (res.Error as CitaError.Validation)?.Errores.Should()
                .Contain("La fecha de matriculación no puede ser una fecha futura.");
        }
        
        [Test]
        [TestCase(-1)]
        [TestCase(31)]
        public void Validar_FechaInspeccionInvalida_RetornaFailure(int diaDesdeHoy) {
            
            // arrange
            var c = new Cita {
                Id = 1,
                Matricula = "1111aaa",
                Dni = "12345678z",
                Marca = "Skoda",
                Modelo = "Octavia",
                Cilindrada = 2000,
                Motor = Cita.TiposMotor.Diesel,
                FechaMatriculacion = DateTime.Now.AddYears(-20),
                FechaInspeccion = DateTime.Now.AddDays(diaDesdeHoy),
            };

            // act
            var res = _validador.Validar(c);
            
            // assert
            res.IsSuccess.Should().BeFalse();
            
            res.Error.Should().BeOfType<CitaError.Validation>();
            
            (res.Error as CitaError.Validation)?.Errores.Should()
                .Contain("La fecha de inspección debe ser desde hoy hasta el límite configurado.");
        }
    }
}