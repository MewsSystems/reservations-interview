using System.Data;
using Dapper;
using Db;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Repositories;

namespace api.Tests.Common;

internal sealed class TestDb : IAsyncDisposable
{
    public SqliteConnection Connection { get; }
    public ReservationRepository ReservationRepository { get; }

    private TestDb(SqliteConnection connection)
    {
        Connection = connection;
        ReservationRepository = new ReservationRepository(connection);
    }

    public static async Task<TestDb> CreateAsync()
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = $"reservation-repository-tests-{Guid.NewGuid()}",
            Mode = SqliteOpenMode.Memory
        }.ToString();

        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddSingleton(connection);
        services.AddSingleton<IDbConnection>(connection);
        var serviceProvider = services.BuildServiceProvider();

        await Setup.EnsureDbAsync(serviceProvider.CreateScope());
        await connection.ExecuteAsync(
            "INSERT INTO Rooms(Number, State) VALUES(@Number, @State);",
            new { Number = 101, State = 0 }
        );

        return new TestDb(connection);
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
    }
}
