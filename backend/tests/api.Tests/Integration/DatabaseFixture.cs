using Microsoft.Extensions.DependencyInjection;
using api.Shared.Extensions;
using System.Data;
using api.Shared.Db;

namespace api.Tests.Integration
{
    public class DatabaseFixture : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        public IDbConnection DbConnection { get; }

        public DatabaseFixture()
        {
            var services = new ServiceCollection();

            services.AddReservationServices("DataSource=file::memory:?cache=shared");
            _serviceProvider = services.BuildServiceProvider();

            DbConnection = _serviceProvider.GetRequiredService<IDbConnection>();
            DbConnection.Open();

            Setup.EnsureDb(_serviceProvider);
        }

        public void Dispose()
        {
            DbConnection.Close();
            DbConnection.Dispose();
            (_serviceProvider as IDisposable)?.Dispose();
        }
    }
}
