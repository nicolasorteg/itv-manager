using System.IO;
using Manager.Cache;
using Manager.Cache.Common;
using Manager.Config;
using Manager.Entity;
using Manager.Models;
using Manager.Repositories.Ado;
using Manager.Repositories.Base;
using Manager.Repositories.Dapper;
using Manager.Repositories.EfCore;
using Manager.Service.Manager;
using Manager.Storage.Common;
using Manager.Storage.Csv;
using Manager.Storage.Json;
using Manager.Storage.Xml;
using Manager.Validators.Citas;
using Manager.Validators.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Manager.Infrastructure;

/// <summary> Inyector de dependencias </summary>
public class DependenciesProvider {
    
    public static IServiceProvider BuildServiceProvider() {
        var services = new ServiceCollection();

        // tareas de mantenimiento iniciales antes de inyectar
        CleanData();

        // registrar cada módulo del ciclo de vida arquitectónico
        RegisterCaches(services);
        RegisterValidators(services);
        RegisterStorages(services);
        RegisterRepositories(services);
        RegisterServices(services);

        return services.BuildServiceProvider();
    }
    
    private static void RegisterCaches(IServiceCollection services) {
        services.AddSingleton<ICache<int, Cita>>(sp => 
            new LruCache<int, Cita>(AppConfig.CacheSize));         // cache global

    }

    private static void RegisterValidators(IServiceCollection services) {
        // el validador se instancia bajo demanda cada vez que se pide
        services.AddTransient<IValidator<Cita>, CitaValidator>();
    }

    private static void RegisterStorages(IServiceCollection services) {
        services.AddTransient<IStorage<Cita>>(sp => {
            var storageType = AppConfig.StorageType.ToLower();
            return storageType switch {
                "csv" => new CitaCsvStorage(),
                "json"  => new CitaJsonStorage(),
                "xml"  => new CitaXmlStorage(),
                _      => new CitaJsonStorage() // json default
            };
        });
    }

    private static void RegisterRepositories(IServiceCollection services) {
        services.AddSingleton<ICitaRepository>(sp => {
            var repoType = AppConfig.RepositoryType.ToLower();
            return repoType switch {
                "ado"    => CreateAdoRepository(AppConfig.DropData, AppConfig.SeedData),
                "efcore" => CreateEfRepository(AppConfig.DropData, AppConfig.SeedData),
                "dapper" => CreateDapperRepository(AppConfig.DropData, AppConfig.SeedData),
                _        => CreateEfRepository(AppConfig.DropData, AppConfig.SeedData) // default efcore
            };
        });
    }

    private static ICitaRepository CreateAdoRepository(bool dropData, bool seedData) {
        AsegurarCarpetaDeDatos();
        var connection = new SqliteConnection(AppConfig.ConnectionString);
        connection.Open();
        return new CitaAdoRepository(dropData, seedData);
    }

    private static ICitaRepository CreateDapperRepository(bool dropData, bool seedData) {
        AsegurarCarpetaDeDatos();
        var connection = new SqliteConnection(AppConfig.ConnectionString);
        connection.Open();
        return new CitaDapperRepository(connection, () => connection.Close(), dropData, seedData);
    }

    private static ICitaRepository CreateEfRepository(bool dropData, bool seedData) {
        AsegurarCarpetaDeDatos();
        var context = new AppDbContext(AppConfig.ConnectionString);
        return new CitaEfCoreRepository(context, dropData, seedData);
    }

    private static void RegisterServices(IServiceCollection services) {
        services.AddScoped<IManagerService, ManagerService>(sp => 
            new ManagerService(
                sp.GetRequiredService<ICitaRepository>(),
                sp.GetRequiredService<IValidator<Cita>>(),
                sp.GetRequiredService<ICache<int, Cita>>()
            ));
    }
    
    
    // métodos auxiliares -------------------------
    private static void AsegurarCarpetaDeDatos() {
        var dataFolder = AppConfig.DataFolder;
        if (!Directory.Exists(dataFolder)) {
            Directory.CreateDirectory(dataFolder);
        }
    }

    private static void CleanData() { 
        // si se activa limpiar la BD o resembrar, vaciamos también la carpeta temporal de reportes
        if (AppConfig.DropData || AppConfig.SeedData) {
            CleanDirectory(AppConfig.ReportDirectory);
        }
    }

    private static void CleanDirectory(string path) {
        try {
            if (Directory.Exists(path)) {
                foreach (var file in Directory.GetFiles(path)) {
                    try { File.Delete(file); } catch { }
                }
                foreach (var dir in Directory.GetDirectories(path)) {
                    try { Directory.Delete(dir, true); } catch { }
                }
            }
            Directory.CreateDirectory(path);
        }
        catch (Exception ex) {
            Console.WriteLine($"Warning: No se pudo limpiar directorio {path}: {ex.Message}");
        }
    }
}