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
            services.AddScoped<IDbConnection>(sp => new SqliteConnection(connectionString));
            return services;
        }

        public static IServiceCollection AddReservationServices(this IServiceCollection services, string connectionString)
        {
            services.AddSqlConnection(connectionString);
            services.AddScoped<IGuestRepository, GuestRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IGuestService, GuestService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IReservationService, ReservationService>();
            return services;
        }

    }
}
