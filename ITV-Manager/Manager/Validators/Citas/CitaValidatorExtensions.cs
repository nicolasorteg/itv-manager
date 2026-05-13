using Manager.Config;
using Serilog;
using Manager.Models;

namespace Manager.Validators.Citas;

/// <summary> Contenedor de funciones de extensión para la validación de los datos de la Cita </summary>
public static class CitaValidatorExtensions {

    /// <summary> Validador de la matrícula </summary>
    /// <param name="matricula">Matrícula a validar</param>
    extension(string matricula) {
        public bool IsValidMatricula() {
            Log.Debug("🔵 Validando Matrícula...");

            const string LetrasProhibidas = "AEIOUQÑ";

            if (string.IsNullOrWhiteSpace(matricula)) return false; // 1

            // limpieza cadena
            var m = matricula.Trim().ToUpper().Replace("-", "").Replace(" ", "");
            if (m.Length != 7) return false; // 2

            if (!int.TryParse(m.Substring(0, 4), out var numeros)) return false; // 3

            for (var i = 4; i < m.Length; i++) {
                if (!char.IsLetter(m[i])) return false; // 4
            }

            var letras = m.Substring(4, 3);
            return !letras.Any(l => LetrasProhibidas.Contains(l)); // 5
        }
    }

    /// <summary> Validador del dni </summary>
    /// <param name="dni">Dni a validar</param>
    extension(string dni) {
        public bool IsValidDni() {
            Log.Debug("🔵 Validando Dni...");
            
            const string LetrasValidas = "TRWAGMYFPDXBNJZSQVHLCKE";
        
            if (string.IsNullOrWhiteSpace(dni)) return false; // 6

            var d = dni.Trim().ToUpper().Replace(" ", "");
            if (d.Length != 9) return false; // 7
            
            if (!int.TryParse(d.Substring(0, 8), out var numero)) return false; // 8
            if (!char.IsLetter(d[8])) return false; // 9
            
            return d[8] == LetrasValidas[numero % 23]; // 10
        }
    }
    
    /// <summary> Validador de la marca </summary>
    /// <param name="marca">Marca a validar</param>
    extension(string marca) {
        public bool IsValidMarca() {
            Log.Debug("🔵 Validando Marca...");
            
            if (string.IsNullOrWhiteSpace(marca)) return false; // 11
            return marca.Trim().Length < 15; // 12
        }
    }
    
    /// <summary> Validador del modelo </summary>
    /// <param name="modelo">Modelo a validar</param>
    extension(string modelo) {
        public bool IsValidModelo() {
            Log.Debug("🔵 Validando Modelo...");
            
            if (string.IsNullOrWhiteSpace(modelo)) return false; // 13
            return modelo.Trim().Length < 15; // 14
        }
    }
    
    /// <summary> Validador del motor </summary>
    /// <param name="motor">Motor a validar</param>
    extension(Cita.TiposMotor motor) {
        public bool IsValidMotor() {
            Log.Debug("🔵 Validando Motor...");
            
            return Enum.IsDefined(motor); // 15
        }
    }
    
    /// <summary> Validador de la cilindrada </summary>
    /// <param name="cilindrada">Cilindrada a validar</param>
    extension(int cilindrada) {
        public bool IsValidCilindrada() {
            Log.Debug("🔵 Validando Cilindrada...");
            
            return cilindrada is > 0 and <= 9000; // 16, 17
        }
    }
    
    /// <summary> Validador de la fecha de matriculación </summary>
    /// <param name="fechaMatriculacion">Fecha de Matriculación a validar</param>
    extension(DateTime fechaMatriculacion) {
        public bool IsValidFechaMatriculacion() {
            Log.Debug("🔵 Validando Fecha de Matriculación...");

            return fechaMatriculacion <= DateTime.Today; // 18
        }
    }
    
    /// <summary> Validador de la fecha de la cita </summary>
    /// <param name="fechaInspeccion">Fecha de Inspección a validar</param>
    extension(DateTime fechaInspeccion) {
        public bool IsValidFechaInspeccion() {
            Log.Debug("🔵 Validando Fecha de Inspección...");
            
            return fechaInspeccion >= DateTime.Today && fechaInspeccion.Date <= DateTime.Today.AddDays(AppConfig.VentanaDiasCita); // 19, 20
        }
    }
}