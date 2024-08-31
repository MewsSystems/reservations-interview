namespace api.Tests.Integration
{
    [CollectionDefinition(nameof(DatabaseCollection))]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
