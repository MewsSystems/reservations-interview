using Dapper;
using Microsoft.Data.Sqlite;
using Models;

namespace Db
{
    public static class Setup
    {
        /// <summary>
        /// Versioned migration system using PRAGMA user_version.
        /// Each migration block runs exactly once; the version number is
        /// persisted in the SQLite file itself.
        /// </summary>
        public static async Task EnsureDb(IServiceScope scope)
        {
            using var db = scope.ServiceProvider.GetRequiredService<SqliteConnection>();

            // SQLite WAL (write-ahead log) go brrrr
            await db.ExecuteAsync("PRAGMA journal_mode = wal;");
            // SQLite does not enforce FKs by default
            await db.ExecuteAsync("PRAGMA foreign_keys = ON;");

            var version = await db.ExecuteScalarAsync<int>("PRAGMA user_version;");

            // ── v1: baseline tables ─────────────────────────────────────
            if (version < 1)
            {
                await db.ExecuteAsync(
                    $@"
                  CREATE TABLE IF NOT EXISTS Guests (
                    {nameof(Guest.Email)} TEXT PRIMARY KEY NOT NULL,
                    {nameof(Guest.Name)} TEXT NOT NULL,
                    {nameof(Guest.Surname)} TEXT
                  );
                "
                );

                await db.ExecuteAsync(
                    $@"
                  CREATE TABLE IF NOT EXISTS Rooms (
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
                    {nameof(Reservation.Start)} TEXT NOT NULL,
                    {nameof(Reservation.End)} TEXT NOT NULL,
                    {nameof(Reservation.CheckedIn)} INT NOT NULL DEFAULT FALSE,
                    {nameof(Reservation.CheckedOut)} INT NOT NULL DEFAULT FALSE,
                    FOREIGN KEY ({nameof(Reservation.GuestEmail)})
                      REFERENCES Guests ({nameof(Guest.Email)}),
                    FOREIGN KEY ({nameof(Reservation.RoomNumber)})
                      REFERENCES Rooms ({nameof(Room.Number)})
                  );
                "
                );

                await db.ExecuteAsync("PRAGMA user_version = 1;");
                version = 1;
            }

            // ── v2: indexes ────────────────────────────────────────────────
            if (version < 2)
            {
                await db.ExecuteAsync(
                    "CREATE INDEX IF NOT EXISTS IX_Reservations_End ON Reservations([End]);"
                );
                await db.ExecuteAsync(
                    "CREATE INDEX IF NOT EXISTS IX_Reservations_RoomNumber_Start_End ON Reservations(RoomNumber, [Start], [End]);"
                );

                await db.ExecuteAsync("PRAGMA user_version = 2;");
                version = 2;
            }

            // ── v3: add IsDirty column to Rooms ────────────────────────────
            if (version < 3)
            {
                await db.ExecuteAsync($"ALTER TABLE Rooms ADD COLUMN {nameof(Room.IsDirty)} INT NOT NULL DEFAULT 0;");

                await db.ExecuteAsync("PRAGMA user_version = 3;");
                version = 3;
            }
        }
    }
}
