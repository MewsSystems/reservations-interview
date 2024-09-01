using Microsoft.Extensions.DependencyInjection;
using api.Shared.Extensions;
using System.Data;
using api.Shared.Db;

namespace api.Tests.Integration
{
    public class DatabaseFixture : IDisposable
    {
        private Lazy<IDbConnection> _lazyDbConnection => new(_serviceProvider.GetRequiredService<IDbConnection>());
        public IDbConnection DbConnection => _lazyDbConnection.Value;

        protected readonly IServiceProvider _serviceProvider;    

        public DatabaseFixture()
        {
            var services = new ServiceCollection();

            services.AddReservationServices("DataSource=file::memory:?cache=shared");
            _serviceProvider = services.BuildServiceProvider();

            using var dbConnection = _serviceProvider.GetRequiredService<IDbConnection>();
            // Ensure the database schema is created and setup
            Setup.EnsureDb(_serviceProvider);
        }

        public void Dispose()
        {
            (_serviceProvider as IDisposable)?.Dispose();
        }
    }
}
