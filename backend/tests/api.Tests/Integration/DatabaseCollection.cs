namespace api.Tests.Integration
{
    [CollectionDefinition(nameof(DatabaseCollection), DisableParallelization = true)]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
