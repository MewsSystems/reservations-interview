using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using api.Shared.Constants;
using api.Shared.Extensions;
using api.Shared.Models.DB;
using api.Shared.Models.Errors;
using Dapper;

namespace api.Shared.Repositories.Dapper
{
    public class RoomRepository : IRoomRepository
    {
        private IDbConnection _db { get; set; }

        public RoomRepository(IDbConnection db)
        {
            _db = db.ThrowIfNull(nameof(IDbConnection));
        }

        public async Task<Room> GetRoom(int roomNumber, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            var conn = connection ?? _db;
            var room = await conn.QueryFirstOrDefaultAsync<Room>(
                "SELECT * FROM Rooms WHERE Number = @roomNumber;",
                new { roomNumber },
                transaction
            );
            // I don't like this, I would rather support nullable
            if (room == null)
            {
                throw new NotFoundException($"Room {roomNumber} not found");
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

        public async Task<Room> CreateRoom(Room newRoom, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            var conn = connection ?? _db;
            var createdRoom = await conn.QuerySingleAsync<Room>(
                "INSERT INTO Rooms(Number, State) Values(@Number, @State) RETURNING *",
                newRoom,
                transaction
            );
            return createdRoom;
        }

        public async Task<bool> DeleteRoom(int roomNumber, IDbTransaction? transaction = null)
        {
            var deleted = await _db.ExecuteAsync(
                "DELETE FROM Rooms WHERE Number = @roomNumber;",
                new { roomNumber },
                transaction
            );
            return deleted > 0;
        }

        public async Task<bool> UpdateRoomStatus(int roomNumber, State state, IDbTransaction? transaction = null)
        {
            var updated = await _db.ExecuteAsync(
                "UPDATE Rooms SET State = @state WHERE Number = @roomNumber;",
                new { state, roomNumber },
                transaction
            );
            return updated > 0;
        }
    }
}
