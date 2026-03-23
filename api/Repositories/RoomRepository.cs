using System.Data;
using Dapper;
using Extensions;
using Models;
using Models.Errors;

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
        /// Find a room by its room number, throwing if not found
        /// </summary>
        /// <exception cref="NotFoundException"></exception>
        public async Task<Room> GetRoom(string roomNumber)
        {
            Room.ValidateRoomNumber(roomNumber);

            var room = await _db.QueryFirstOrDefaultAsync<Room>(
                "SELECT * FROM Rooms WHERE Number = @roomNumber;",
                new { roomNumber }
            );

            if (room == null)
            {
                throw new NotFoundException(nameof(Room), roomNumber);
            }

            return room;
        }

        public async Task<IEnumerable<Room>> GetRooms()
        {
            var rooms = await _db.QueryAsync<Room>("SELECT * FROM Rooms");

            if (rooms == null)
            {
                return [];
            }

            return rooms;
        }

        public async Task<Room> CreateRoom(Room newRoom)
        {
            Room.ValidateRoomNumber(newRoom.Number);

            var createdRoom = await _db.QuerySingleAsync<Room>(
                "INSERT INTO Rooms(Number, State) Values(@Number, @State) RETURNING *",
                newRoom
            );

            return createdRoom;
        }

        public async Task<int> CreateRoomsBatch(List<(int Line, Room Room)> rooms, List<Contracts.ImportError> errors)
        {
            if (rooms.Count == 0) return 0;

            using var transaction = _db.BeginSerializableTransaction();
            var imported = 0;

            try
            {
                var existingNumbers = (await _db.QueryAsync<string>(
                    "SELECT Number FROM Rooms WHERE Number IN @Numbers",
                    new { Numbers = rooms.Select(r => r.Room.Number).ToList() },
                    transaction
                )).ToHashSet();

                foreach (var (line, room) in rooms)
                {
                    if (existingNumbers.Contains(room.Number))
                    {
                        errors.Add(new Contracts.ImportError(line, room.Number, $"Room {room.Number} already exists"));
                        continue;
                    }

                    await _db.ExecuteAsync(
                        "INSERT INTO Rooms(Number, State) VALUES(@Number, @State)",
                        room,
                        transaction
                    );
                    imported++;
                }

                transaction.Commit();
                return imported;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> RoomExists(string roomNumber)
        {
            var count = await _db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Rooms WHERE Number = @roomNumber",
                new { roomNumber }
            );
            return count > 0;
        }

        public async Task<Room> UpdateRoomState(string roomNumber, State state)
        {
            var room = await GetRoom(roomNumber);

            await _db.ExecuteAsync(
                "UPDATE Rooms SET State = @State WHERE Number = @roomNumber",
                new { State = (int)state, roomNumber }
            );

            room.State = state;
            return room;
        }

        public async Task<bool> DeleteRoom(string roomNumber)
        {
            Room.ValidateRoomNumber(roomNumber);

            var deleted = await _db.ExecuteAsync(
                "DELETE FROM Rooms WHERE Number = @roomNumber;",
                new { roomNumber }
            );

            return deleted > 0;
        }
    }
}
