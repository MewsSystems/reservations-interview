using api.Shared.Services.Core;

namespace api.Shared.Services
{
    public class DbConnectionStringService : IDbConnectionStringService
    {
        private readonly string _connectionString;

        public DbConnectionStringService(string connectionString)
        {
            _connectionString = connectionString;
        }
        public string GetConnectionString()
        {
            return _connectionString;
        }

    }
}
