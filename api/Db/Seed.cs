using Dapper;
using Microsoft.Data.Sqlite;
using Models;

namespace Db
{
    public static class Seed
    {
        public static async Task SeedData(SqliteConnection db)
        {
            await SeedRooms(db);
        }

        private static async Task SeedRooms(SqliteConnection db)
        {
            var existingRooms = await db.QueryFirstOrDefaultAsync<int>("SELECT COUNT(*) FROM Rooms");
            if (existingRooms > 0) return;

            var rooms = new[] { "101", "102", "103", "104", "105", "201", "202", "203" };
            foreach (var room in rooms)
            {
                await db.ExecuteAsync(
                    "INSERT OR IGNORE INTO Rooms(Number, State) VALUES(@Number, @State)",
                    new { Number = room, State = (int)State.Ready }
                );
            }
        }
    }
}
