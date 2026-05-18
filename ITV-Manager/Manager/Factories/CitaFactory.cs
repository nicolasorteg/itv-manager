using Manager.Models;

namespace Manager.Factories;

public class CitaFactory {
    
    /// <summary> Semilla de datos iniciales </summary>
    /// <returns>Citas iniciales</returns>
    public static IEnumerable<Cita> Seed() {
        var hoy = DateTime.Today;
        return new List<Cita> 
        {
            // borradas
            new Cita { Id = 1, Matricula = "1001-AAA", Marca = "Toyota Corolla", Cilindrada = 1800, Motor = Cita.TiposMotor.Hibrido, Dni = "12345678A", FechaMatriculacion = hoy.AddYears(-4), FechaInspeccion = new DateTime(2024, 02, 15), IsDeleted = true },
            new Cita { Id = 2, Matricula = "1002-BBB", Marca = "BMW M3", Cilindrada = 3000, Motor = Cita.TiposMotor.Gasolina, Dni = "23456789B", FechaMatriculacion = hoy.AddYears(-3), FechaInspeccion = new DateTime(2024, 05, 20), IsDeleted = true },
            new Cita { Id = 3, Matricula = "1003-CCC", Marca = "Tesla Model 3", Cilindrada = 0, Motor = Cita.TiposMotor.Electrico, Dni = "34567890C", FechaMatriculacion = hoy.AddYears(-2), FechaInspeccion = new DateTime(2024, 08, 10), IsDeleted = true },
            new Cita { Id = 4, Matricula = "1004-DDD", Marca = "Audi A3", Cilindrada = 2000, Motor = Cita.TiposMotor.Diesel, Dni = "45678901D", FechaMatriculacion = hoy.AddYears(-5), FechaInspeccion = new DateTime(2024, 11, 05), IsDeleted = true },
            new Cita { Id = 5, Matricula = "1005-EEE", Marca = "Seat Leon", Cilindrada = 1500, Motor = Cita.TiposMotor.Gasolina, Dni = "56789012E", FechaMatriculacion = hoy.AddYears(-1), FechaInspeccion = new DateTime(2024, 12, 28), IsDeleted = true },
            new Cita { Id = 6, Matricula = "1006-FFF", Marca = "Ford Focus", Cilindrada = 1600, Motor = Cita.TiposMotor.Diesel, Dni = "67890123F", FechaMatriculacion = hoy.AddYears(-4), FechaInspeccion = new DateTime(2024, 03, 12), IsDeleted = true },
            new Cita { Id = 7, Matricula = "1007-GGG", Marca = "Mercedes Clase A", Cilindrada = 2000, Motor = Cita.TiposMotor.Diesel, Dni = "78901234G", FechaMatriculacion = hoy.AddYears(-3), FechaInspeccion = new DateTime(2024, 06, 18), IsDeleted = true },
            new Cita { Id = 8, Matricula = "1008-HHH", Marca = "Hyundai Tucson", Cilindrada = 1600, Motor = Cita.TiposMotor.Hibrido, Dni = "89012345H", FechaMatriculacion = hoy.AddYears(-2), FechaInspeccion = new DateTime(2024, 09, 25), IsDeleted = true },
            new Cita { Id = 9, Matricula = "1021-UUU", Marca = "Toyota Yaris", Cilindrada = 1500, Motor = Cita.TiposMotor.Hibrido, Dni = "11223344U", FechaMatriculacion = hoy.AddYears(-2), FechaInspeccion = new DateTime(2025, 01, 15), IsDeleted = true },
            new Cita { Id = 10, Matricula = "1022-VVV", Marca = "BMW X1", Cilindrada = 2000, Motor = Cita.TiposMotor.Diesel, Dni = "22334455V", FechaMatriculacion = hoy.AddYears(-3), FechaInspeccion = new DateTime(2025, 02, 20), IsDeleted = true },
            new Cita { Id = 11, Matricula = "1023-WWW", Marca = "Tesla Model Y", Cilindrada = 0, Motor = Cita.TiposMotor.Electrico, Dni = "33445566W", FechaMatriculacion = hoy.AddYears(-1), FechaInspeccion = new DateTime(2025, 03, 10), IsDeleted = true },
            new Cita { Id = 12, Matricula = "1024-XXX", Marca = "Audi Q3", Cilindrada = 2000, Motor = Cita.TiposMotor.Gasolina, Dni = "44556677X", FechaMatriculacion = hoy.AddYears(-4), FechaInspeccion = new DateTime(2025, 04, 05), IsDeleted = true },
            new Cita { Id = 13, Matricula = "1025-YYY", Marca = "Seat Ateca", Cilindrada = 1600, Motor = Cita.TiposMotor.Diesel, Dni = "55667788Y", FechaMatriculacion = hoy.AddYears(-3), FechaInspeccion = new DateTime(2025, 05, 12), IsDeleted = true },
            new Cita { Id = 14, Matricula = "1026-ZZZ", Marca = "Ford Kuga", Cilindrada = 2500, Motor = Cita.TiposMotor.Hibrido, Dni = "66778899Z", FechaMatriculacion = hoy.AddYears(-2), FechaInspeccion = new DateTime(2025, 06, 18), IsDeleted = true },
            new Cita { Id = 15, Matricula = "1027-AAA", Marca = "Mercedes GLC", Cilindrada = 2200, Motor = Cita.TiposMotor.Diesel, Dni = "77889900A", FechaMatriculacion = hoy.AddYears(-4), FechaInspeccion = new DateTime(2025, 07, 25), IsDeleted = true },
            new Cita { Id = 16, Matricula = "1028-BBB", Marca = "Hyundai Ioniq", Cilindrada = 0, Motor = Cita.TiposMotor.Electrico, Dni = "88990011B", FechaMatriculacion = hoy.AddYears(-1), FechaInspeccion = new DateTime(2025, 08, 30), IsDeleted = true },
            new Cita { Id = 17, Matricula = "1029-CCC", Marca = "Kia Niro", Cilindrada = 1600, Motor = Cita.TiposMotor.Hibrido, Dni = "99001122C", FechaMatriculacion = hoy.AddYears(-3), FechaInspeccion = new DateTime(2025, 09, 14), IsDeleted = true },
            new Cita { Id = 18, Matricula = "1030-DDD", Marca = "Nissan Juke", Cilindrada = 1000, Motor = Cita.TiposMotor.Gasolina, Dni = "00112233D", FechaMatriculacion = hoy.AddYears(-2), FechaInspeccion = new DateTime(2025, 10, 22), IsDeleted = true },
            
            // no borradas
            new Cita { Id = 19, Matricula = "2041-AAA", Marca = "Renault Clio", Cilindrada = 1200, Motor = Cita.TiposMotor.Gasolina, Dni = "41000001A", FechaMatriculacion = hoy.AddYears(-6), FechaInspeccion = new DateTime(2026, 06, 10), IsDeleted = false },
            new Cita { Id = 20, Matricula = "2042-BBB", Marca = "Peugeot 208", Cilindrada = 1400, Motor = Cita.TiposMotor.Diesel, Dni = "42000002B", FechaMatriculacion = hoy.AddYears(-2), FechaInspeccion = new DateTime(2026, 06, 15), IsDeleted = false },
            new Cita { Id = 21, Matricula = "2043-CCC", Marca = "Kia Ceed", Cilindrada = 1600, Motor = Cita.TiposMotor.Hibrido, Dni = "43000003C", FechaMatriculacion = hoy.AddYears(-3), FechaInspeccion = new DateTime(2026, 06, 12), IsDeleted = false },
            new Cita { Id = 22, Matricula = "2044-DDD", Marca = "Hyundai i30", Cilindrada = 1500, Motor = Cita.TiposMotor.Gasolina, Dni = "44000004D", FechaMatriculacion = hoy.AddYears(-1), FechaInspeccion = new DateTime(2026, 05, 30), IsDeleted = false },
            new Cita { Id = 23, Matricula = "2045-EEE", Marca = "Mazda 3", Cilindrada = 2000, Motor = Cita.TiposMotor.Gasolina, Dni = "45000005E", FechaMatriculacion = hoy.AddYears(-4), FechaInspeccion = new DateTime(2026, 05, 31), IsDeleted = false },
            new Cita { Id = 24, Matricula = "2046-FFF", Marca = "Nissan Leaf", Cilindrada = 0, Motor = Cita.TiposMotor.Electrico, Dni = "46000006F", FechaMatriculacion = hoy.AddYears(-2), FechaInspeccion = new DateTime(2026, 05, 29), IsDeleted = false },
            new Cita { Id = 25, Matricula = "2047-GGG", Marca = "Volvo XC40", Cilindrada = 1500, Motor = Cita.TiposMotor.Hibrido, Dni = "47000007G", FechaMatriculacion = hoy.AddYears(-3), FechaInspeccion = new DateTime(2026, 05, 31), IsDeleted = false },
            new Cita { Id = 26, Matricula = "2048-HHH", Marca = "Skoda Fabia", Cilindrada = 1000, Motor = Cita.TiposMotor.Gasolina, Dni = "48000008H", FechaMatriculacion = hoy.AddYears(-5), FechaInspeccion = new DateTime(2026, 06, 17), IsDeleted = false },
            new Cita { Id = 27, Matricula = "2049-III", Marca = "Fiat 500", Cilindrada = 0, Motor = Cita.TiposMotor.Electrico, Dni = "49000009I", FechaMatriculacion = hoy.AddYears(-1), FechaInspeccion = new DateTime(2026, 06, 18), IsDeleted = false },
            new Cita { Id = 28, Matricula = "2050-JJJ", Marca = "Opel Corsa", Cilindrada = 1200, Motor = Cita.TiposMotor.Gasolina, Dni = "50000010J", FechaMatriculacion = hoy.AddYears(-2), FechaInspeccion = new DateTime(2026, 06, 05), IsDeleted = false },
            new Cita { Id = 29, Matricula = "3051-AAA", Marca = "Porsche Taycan", Cilindrada = 0, Motor = Cita.TiposMotor.Electrico, Dni = "51000001A", FechaMatriculacion = hoy.AddYears(-2), FechaInspeccion = new DateTime(2026, 06, 12), IsDeleted = false },
            new Cita { Id = 30, Matricula = "3052-BBB", Marca = "Mercedes Clase C", Cilindrada = 2000, Motor = Cita.TiposMotor.Diesel, Dni = "52000002B", FechaMatriculacion = hoy.AddYears(-3), FechaInspeccion = new DateTime(2026, 06, 11), IsDeleted = false },
            new Cita { Id = 31, Matricula = "3053-CCC", Marca = "Audi A4", Cilindrada = 2000, Motor = Cita.TiposMotor.Gasolina, Dni = "53000003C", FechaMatriculacion = hoy.AddYears(-4), FechaInspeccion = new DateTime(2026, 06, 17), IsDeleted = false },
            new Cita { Id = 32, Matricula = "3054-DDD", Marca = "Toyota Prius", Cilindrada = 1800, Motor = Cita.TiposMotor.Hibrido, Dni = "54000004D", FechaMatriculacion = hoy.AddYears(-5), FechaInspeccion = new DateTime(2026, 06, 16), IsDeleted = false },
            new Cita { Id = 33, Matricula = "3055-EEE", Marca = "BMW iX", Cilindrada = 0, Motor = Cita.TiposMotor.Electrico, Dni = "55000005E", FechaMatriculacion = hoy.AddYears(-1), FechaInspeccion = new DateTime(2026, 06, 15), IsDeleted = false },
            new Cita { Id = 34, Matricula = "3056-FFF", Marca = "Seat Tarraco", Cilindrada = 2000, Motor = Cita.TiposMotor.Diesel, Dni = "56000006F", FechaMatriculacion = hoy.AddYears(-2), FechaInspeccion = new DateTime(2026, 06, 15), IsDeleted = false },
            new Cita { Id = 35, Matricula = "3057-GGG", Marca = "Ford Mustang", Cilindrada = 5000, Motor = Cita.TiposMotor.Gasolina, Dni = "57000007G", FechaMatriculacion = hoy.AddYears(-3), FechaInspeccion = new DateTime(2026, 06, 14), IsDeleted = false },
            new Cita { Id = 36, Matricula = "3058-HHH", Marca = "Hyundai Ioniq 5", Cilindrada = 0, Motor = Cita.TiposMotor.Electrico, Dni = "58000008H", FechaMatriculacion = hoy.AddYears(-1), FechaInspeccion = new DateTime(2026, 06, 13), IsDeleted = false }
        };
    }
}