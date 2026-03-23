using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace api.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string ConnectionString = "Data Source=IntegrationTest;Mode=Memory;Cache=Shared";

    // keep one dummy connection alive for the lifetime of the factory
    // so the in-memory DB is not destroyed between scoped connections
    private readonly SqliteConnection _keepAlive = new(ConnectionString);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _keepAlive.Open();

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(SqliteConnection) || d.ServiceType == typeof(IDbConnection))
                .ToList();
            foreach (var d in descriptors) services.Remove(d);

            // each scope gets its own open connection
            services.AddScoped(_ =>
            {
                var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                return conn;
            });
            services.AddScoped<IDbConnection>(sp => sp.GetRequiredService<SqliteConnection>());
        });
    }

    protected override void Dispose(bool disposing)
    {
        _keepAlive.Close();
        base.Dispose(disposing);
    }
}
