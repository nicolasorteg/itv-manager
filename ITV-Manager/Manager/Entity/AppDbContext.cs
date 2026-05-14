using Microsoft.EntityFrameworkCore;

namespace Manager.Entity;

/// <summary>
/// Puente que conecta C# con SQLite para Entity Framework Core. ORM
/// </summary>
public class AppDbContext: DbContext {
    
    public DbSet<CitaEntity> Citas { get; set; } = null!; // EF se encarga de inicializar
    
    private readonly string _connectionString; // ruta de la bd
    
    // constructor para pasar ruta manualmente
    public AppDbContext(string path) {
        _connectionString = path;
    }
    
    // constructor desde appsettings.json
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        _connectionString = string.Empty;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        // si la bd no esta configurada desde fuera y el path existe usamos sqlite
        if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(_connectionString)) {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }

    // comprueba si la bd existe y si no la crea
    public void EnsureCreated() {
        Database.EnsureCreated();
    }
}