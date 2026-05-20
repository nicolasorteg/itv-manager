namespace Manager.Test.Storage.Csv;

[TestFixture]
public class CitaCsvStorageTest {
    
    private CitaCsvStorageTest _storage;
    private string _tempPath;
    
    [SetUp]
    public void SetUp() {
        _storage = new CitaCsvStorageTest();
        _tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
    }
}