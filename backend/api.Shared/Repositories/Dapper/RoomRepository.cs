using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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

        public async Task<Room> GetRoom(int roomNumber)
        {
            var room = await _db.QueryFirstOrDefaultAsync<Room>(
                "SELECT * FROM Rooms WHERE Number = @roomNumber;",
                new { roomNumber }
            );
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

        public async Task<Room> CreateRoom(Room newRoom)
        {
            var createdRoom = await _db.QuerySingleAsync<Room>(
                "INSERT INTO Rooms(Number, State) Values(@Number, @State) RETURNING *",
                newRoom
            );
            return createdRoom;
        }

        public async Task<bool> DeleteRoom(int roomNumber)
        {
            var deleted = await _db.ExecuteAsync(
                "DELETE FROM Rooms WHERE Number = @roomNumber;",
                new { roomNumber }
            );
            return deleted > 0;
        }
    }
}
