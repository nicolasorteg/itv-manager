using Serilog;

namespace Manager.Validators.Cita;

public static class CitaValidatorExtensions {

    public static bool IsValidMatricula(this string matricula) {
        Log.Debug("🔵 Validando Matrícula...");
        
        const string LetrasProhibidas = "AEIOUÑQ";
        
        if (string.IsNullOrWhiteSpace(matricula)) return false; // 1
        
        // limpieza cadena
        var m = matricula.Trim().ToUpper().Replace("-", "").Replace(" ", "");
        if (m.Length != 7) return false; // 2

        if (!int.TryParse(m.Substring(0, 4), out var numeros)) return false; // 3
        
        for (var i = 4; i < m.Length; i++) {
            if (!char.TryParse(m[i].ToString(), out var letra)) return false; // 4
        }

        var letras = m.Substring(4, 3);
        return !letras.Any(l => LetrasProhibidas.Contains(l)); // 5
    }

    public static bool IsValidDni(this string dni) {
        Log.Debug("🔵 Validando Dni...");
        
        
        
        
        
        
    }


    
}
