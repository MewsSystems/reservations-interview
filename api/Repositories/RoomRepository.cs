using System.Data;
using Dapper;
using Models;
using Models.Errors;
using Extensions;

namespace Repositories
{
    public class RoomRepository
    {
        private IDbConnection _db { get; set; }

        public RoomRepository(IDbConnection db)
        {
            _db = db;
        }

        /// <summary>
        /// Find a room by its formatted room number, throwing if not found
        /// </summary>
        /// <param name="roomNumber"></param>
        /// <returns cref="Room">An existing room</returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<Room> GetRoom(string roomNumber)
        {
            var roomNumberInt = RoomExtensions.ConvertRoomNumberToInt(roomNumber);

            var room = await _db.QueryFirstOrDefaultAsync<RoomDb>(
                "SELECT * FROM Rooms WHERE Number = @roomNumberInt;",
                new { roomNumberInt }
            );

            if (room == null)
            {
                throw new NotFoundException($"Room {roomNumber} not found");
            }

            return room.ToDomain();
        }

        public async Task<IEnumerable<Room>> GetRooms()
        {
            var rooms = await _db.QueryAsync<RoomDb>("SELECT * FROM Rooms");

            if (rooms == null)
            {
                return [];
            }

            return rooms.Select(r => r.ToDomain());
        }

        public async Task<Room> CreateRoom(Room newRoom)
        {
            var createdRoom = await _db.QuerySingleAsync<RoomDb>(
                "INSERT INTO Rooms(Number, State, IsDirty) Values(@Number, @State, @IsDirty) RETURNING *",
                new RoomDb(newRoom)
            );

            return createdRoom.ToDomain();
        }

        public async Task<bool> SetRoomDirtyState(string roomNumber, bool isDirty)
        {
            var roomNumberInt = RoomExtensions.ConvertRoomNumberToInt(roomNumber);

            var updated = await _db.ExecuteAsync(
                "UPDATE Rooms SET IsDirty = @isDirty WHERE Number = @roomNumberInt;",
                new { isDirty, roomNumberInt }
            );

            return updated > 0;
        }

        public async Task<bool> UpdateRoomState(string roomNumber, State state)
        {
            var roomNumberInt = RoomExtensions.ConvertRoomNumberToInt(roomNumber);

            var updated = await _db.ExecuteAsync(
                "UPDATE Rooms SET State = @state WHERE Number = @roomNumberInt;",
                new { state = (int)state, roomNumberInt }
            );

            return updated > 0;
        }

        /// <summary>
        /// Returns the set of all room numbers currently in the database (as ints) for O(1) duplicate checking.
        /// </summary>
        public async Task<HashSet<int>> GetExistingRoomNumbers()
        {
            var numbers = await _db.QueryAsync<int>("SELECT Number FROM Rooms;");
            return new HashSet<int>(numbers);
        }

        /// <summary>
        /// Batch-inserts rooms in a single transaction for performance.
        /// Callers are responsible for pre-filtering duplicates and invalid rooms.
        /// </summary>
        public async Task<int> BulkCreateRooms(IEnumerable<Room> rooms, CancellationToken ct = default)
        {
            if (_db.State != ConnectionState.Open) _db.Open();

            using var txn = _db.BeginTransaction();

            try
            {
                var dbRooms = rooms.Select(r => new RoomDb(r)).ToList();
                var sql = "INSERT INTO Rooms(Number, State, IsDirty) VALUES(@Number, @State, @IsDirty)";

                var cmd = new CommandDefinition(
                    sql,
                    dbRooms,
                    transaction: txn,
                    cancellationToken: ct
                );

                var affected = await _db.ExecuteAsync(cmd);
                txn.Commit();
                return affected;
            }
            catch
            {
                txn.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteRoom(string roomNumber)
        {
            var roomNumberInt = RoomExtensions.ConvertRoomNumberToInt(roomNumber);

            var deleted = await _db.ExecuteAsync(
                "DELETE FROM Rooms WHERE Number = @roomNumberInt;",
                new { roomNumberInt }
            );

            return deleted > 0;
        }

        // Inner class to hide the details of a direct mapping to SQLite
        private class RoomDb
        {
            /// <summary>
            /// PKID For Rooms. SQLite stores as an integer
            /// </summary>
            public int Number { get; set; }

            /// <summary>
            /// Whether the room is available for reservation
            /// </summary>
            public State State { get; set; } = State.Ready;

            public bool IsDirty { get; set; } = false;

            public RoomDb() { }

            public RoomDb(Room room)
            {
                Number = RoomExtensions.ConvertRoomNumberToInt(room.Number);
                State = room.State;
                IsDirty = room.IsDirty;
            }

            public Room ToDomain()
            {
                return new Room { Number = RoomExtensions.FormatRoomNumber(Number), State = State, IsDirty = IsDirty };
            }
        }
    }
}
