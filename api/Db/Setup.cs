using Dapper;
using System.Data;
using Models;

namespace Db
{
    public static class Setup
    {
        /// <summary>
        /// Ensures the DB is available and the requried tables are made
        /// </summary>
        public static async Task EnsureDbAsync(IServiceScope scope)
        {
            var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();

            await db.ExecuteAsync(
                $@"
              CREATE TABLE IF NOT EXISTS Guests (
                {nameof(Guest.Email)} TEXT PRIMARY KEY NOT NULL,
                {nameof(Guest.Name)} TEXT NOT NULL
              );
            "
            );

            await db.ExecuteAsync(
                $@"
              CREATE TABLE IF NOT Exists Rooms (
                {nameof(Room.Number)} INT PRIMARY KEY NOT NULL,
                {nameof(Room.State)} INT NOT NULL
              );
            "
            );

            await db.ExecuteAsync(
                $@"
              CREATE TABLE IF NOT EXISTS Reservations (
                {nameof(Reservation.Id)} TEXT PRIMARY KEY NOT NULL,
                {nameof(Reservation.GuestEmail)} TEXT NOT NULL,
                {nameof(Reservation.RoomNumber)} INT NOT NULL,
                {nameof(Reservation.Start)} INT NOT NULL,
                {nameof(Reservation.End)} INT NOT NULL,
                {nameof(Reservation.CheckedIn)} INT NOT NULL DEFAULT FALSE,
                {nameof(Reservation.CheckedOut)} INT NOT NULL DEFAULT FALSE,
                FOREIGN KEY ({nameof(Reservation.GuestEmail)})
                  REFERENCES Guests ({nameof(Guest.Email)}),
                FOREIGN KEY ({nameof(Reservation.RoomNumber)})
                  REFERENCES Rooms ({nameof(Room.Number)})
              );
            "
            );
        }
    }
}
