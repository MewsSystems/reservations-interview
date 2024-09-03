namespace api.Tests.Integration
{
    [CollectionDefinition(nameof(DatabaseSeedCollection), DisableParallelization = true)]
    public class DatabaseSeedCollection : ICollectionFixture<DatabaseSeedFixture>
    {
    }
}
