using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Manager.Config;

/// <summary> Clase de configuración que lee el appsettings.json </summary>
public static class AppConfig {
    
    public static IConfigurationRoot Configuration { get; }
    
    // encendido
    static AppConfig() {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true) // archivo no opcional y cambios reactivos
            .Build();
    }
    
    public static CultureInfo Locale => CultureInfo.GetCultureInfo("es-ES");
    public static string AppName => Configuration.GetValue("AppName", "ITV-Manager");
    public static string Version => Configuration.GetValue("Version", "1.0.0");
    
    
    // config repositorio y datos
    public static string DataFolder => Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, // portable
        Configuration.GetValue<string>("Repository:Directory") ?? "data");
    
    public static string ConnectionString => 
        Configuration.GetValue<string>("Repository:ConnectionString") ?? "Data Source=data/itv.db";
    
    public static string StorageType => Configuration.GetValue<string>("Storage:Type") ?? "json";

    // RNF-04, RN-07
    public static string RepositoryType {
        get {
            var type = Configuration.GetValue<string>("Repository:Type") ?? "efcore"; //default efcore
            return type.ToLower() switch {
                "ado" or "adonet" => "ado",
                "dapper" => "dapper",
                "efcore" => "efcore",
                "json" => "json",
                _ => "efcore"
            };
        }
    }
    
    public static string ItvDataFile {
        get {
            var extension = StorageType.ToLower() switch {
                "json" => "json",
                "xml" => "xml",
                "csv" => "csv",
                _ => "json"
            };
            return Path.Combine(DataFolder, $"gestionITV.{extension}");
        }
    }
    
    public static bool DropData => Configuration.GetValue("Repository:DropData", false);
    public static bool SeedData => Configuration.GetValue("Repository:SeedData", true);
    public static bool UseLogicalDelete => Configuration.GetValue("Repository:UseLogicalDelete", true);
    
    
    // reglas de negocio, evitando hardcoding
    public static int MaxVehiculosPorDni => Configuration.GetValue("Citas:MaxVehiculosPorDiaPorDni", 3);
    public static int VentanaDiasCita => Configuration.GetValue("Citas:VentanaDiasCita", 30);
    
    // caché
    public static int CacheSize => Configuration.GetValue("Cache:Size", 10);
    
    // backup
    public static string BackupDirectory => Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        Configuration.GetValue<string>("Backup:Directory") ?? "backup");

    public static string BackupFormat => Configuration.GetValue("Backup:Format", "json").ToLower();
    
    // reporte
    public static string ReportDirectory => Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        Configuration.GetValue<string>("Reports:Directory") ?? "reports");
}