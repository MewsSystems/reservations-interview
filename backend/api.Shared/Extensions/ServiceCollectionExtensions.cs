using api.Shared.Models.Domain;
using api.Shared.Repositories;
using api.Shared.Repositories.Dapper;
using api.Shared.Services;
using api.Shared.Validation.Domain;
using FluentValidation;
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
            services.AddSingleton<IGuestRepository, GuestRepository>();
            services.AddSingleton<IRoomRepository, RoomRepository>();
            services.AddSingleton<IReservationRepository, ReservationRepository>();
            services.AddSingleton<IGuestService, GuestService>();
            services.AddSingleton<IRoomService, RoomService>();
            services.AddSingleton<IReservationService, ReservationService>();
            return services;
        }

    }
}
