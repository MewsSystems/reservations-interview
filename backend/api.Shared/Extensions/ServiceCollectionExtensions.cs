using api.Shared.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace api.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlConnection(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IDbConnection>(sp => new SqliteConnection(connectionString));
            return services;
        }

        public static IServiceCollection AddReservationServices(this IServiceCollection services, string connectionString)
        {
            services.AddSqlConnection(connectionString);
            services.AddSingleton<GuestRepository>();
            services.AddSingleton<RoomRepository>();
            services.AddSingleton<ReservationRepository>();
            return services;
        }
    }
}
