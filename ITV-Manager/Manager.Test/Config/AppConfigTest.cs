using FluentAssertions;
using Manager.Config;
using Microsoft.Extensions.Configuration;

namespace Manager.Test.Config;

public class AppConfigTest {
    
    [Test]
    public void Locale_DeberiaRetornarEspana() {
        AppConfig.Locale.Should().NotBeNull();
        AppConfig.Locale.Name.Should().Be("es-ES");
    }

    [Test]
    public void AppName_DeberiaRetornarValorOValorPorDefecto() {
        AppConfig.AppName.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void Version_DeberiaRetornarVersionValida() {
        AppConfig.Version.Should().NotBeNullOrEmpty();
    }
    
    [Test]
    public void Flags_DeberianRetornarBooleanos() {
        AppConfig.DropData.Should().Be(AppConfig.Configuration.GetValue("Repository:DropData", false));
        AppConfig.SeedData.Should().Be(AppConfig.Configuration.GetValue("Repository:SeedData", true));
        AppConfig.UseLogicalDelete.Should().Be(AppConfig.Configuration.GetValue("Repository:UseLogicalDelete", true));
    }

    [Test]
    public void ConnectionString_DeberiaContenerDataSource() {
        AppConfig.ConnectionString.Should().NotBeNullOrEmpty();
        AppConfig.ConnectionString.Should().Contain("Data Source");
    }

    [Test]
    public void StorageType_DeberiaRetornarTipoValido() {
        AppConfig.StorageType.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void BackupDirectory_DeberiaRetornarRutaValida() {
        AppConfig.BackupDirectory.Should().NotBeNullOrEmpty();
        Path.IsPathRooted(AppConfig.BackupDirectory).Should().BeTrue();
    }

    [Test]
    public void BackupFormat_DeberiaEstarEnMinusculas() {
        AppConfig.BackupFormat.Should().Be(AppConfig.BackupFormat.ToLower());
    }

    [Test]
    public void ReportDirectory_DeberiaRetornarRutaValida() {
        AppConfig.ReportDirectory.Should().NotBeNullOrEmpty();
        Path.IsPathRooted(AppConfig.ReportDirectory).Should().BeTrue();
    }

    [Test]
    public void RepositoryType_DeberiaCubrirTodasLasRamasDelSwitch() {

        switch (AppConfig.Configuration["Repository:Type"]) {
            case "ado":
            case "adonet":
                AppConfig.RepositoryType.Should().Be("ado");
                break;
            case "dapper":
                AppConfig.RepositoryType.Should().Be("dapper");
                break;
            case "efcore":
            case "invent":
            case null:
                AppConfig.RepositoryType.Should().Be("efcore");
                break;
        }
    }
    
    [Test]
    public void ItvDataFile_DeberiaCubrirTodasLasRamasDelSwitch() {

        switch (AppConfig.Configuration["Storage:Type"]) {
            case "xml":
                AppConfig.ItvDataFile.Should().Be("xml");
                break;
            case "csv":
                AppConfig.ItvDataFile.Should().Be("csv");
                break;
            case "json":
            case "invent":
            case null:
                AppConfig.ItvDataFile.Should().Be("json");
                break;
        }
    }

}