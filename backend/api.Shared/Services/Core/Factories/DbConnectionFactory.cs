using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Data;

namespace api.Shared.Services.Core.Factories
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly ILogger<DbConnectionFactory> _logger;
        private readonly IDbConnectionStringService _connectionStringService;

        public DbConnectionFactory(ILogger<DbConnectionFactory> logger, IDbConnectionStringService connectionStringService)
        {
            _logger = logger;
            _connectionStringService = connectionStringService;
        }

        public IDbConnection Get()
        {
            _logger.LogDebug("Creating new {conn} connection ...", nameof(SqliteConnection));
            return new SqliteConnection(_connectionStringService.GetConnectionString());
        }
    }
}
