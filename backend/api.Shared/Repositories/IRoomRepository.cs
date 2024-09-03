using api.Shared.Constants;
using api.Shared.Models.DB;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace api.Shared.Repositories
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetRooms();

        Task<Room> GetRoom(int roomNumber, IDbConnection? connection = null, IDbTransaction? transaction = null);

        Task<Room> CreateRoom(Room newRoom, IDbConnection? connection = null, IDbTransaction? transaction = null);

        Task<bool> DeleteRoom(int roomNumber, IDbTransaction? dbTransaction = null);

        Task<bool> UpdateRoomStatus(int roomNumber, State state, IDbTransaction? dbTransaction = null);
    }
}