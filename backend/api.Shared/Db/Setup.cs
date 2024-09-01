using api.Shared.Models.Domain;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;

namespace api.Shared.Db
{
    public static class Setup
    {
        /// <summary>
        /// Ensures the DB is available and the requried tables are made
        /// </summary>
        public static void EnsureDb(IServiceProvider provider)
        {
            using var db = provider.GetRequiredService<IDbConnection>();

            // SQLite WAL (write-ahead log) go brrrr
            db.Execute("PRAGMA journal_mode = wal;");
            // SQLite does not enforce FKs by default
            db.Execute("PRAGMA foreign_keys = ON;");

            db.Execute(
                $@"
              CREATE TABLE IF NOT EXISTS Guests (
                {nameof(Guest.Email)} TEXT PRIMARY KEY NOT NULL,
                {nameof(Guest.Name)} TEXT NOT NULL
              );
            "
            );

            db.Execute(
                $@"
              CREATE TABLE IF NOT Exists Rooms (
                {nameof(Room.Number)} INT PRIMARY KEY NOT NULL,
                {nameof(Room.State)} INT NOT NULL
              );
            "
            );

            db.Execute(
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
