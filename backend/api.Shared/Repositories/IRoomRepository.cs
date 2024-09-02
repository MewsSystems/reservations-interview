using api.Shared.Constants;
using api.Shared.Models.DB;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace api.Shared.Repositories
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetRooms();

        Task<Room> GetRoom(int roomNumber);

        Task<Room> CreateRoom(Room newRoom);

        Task<bool> DeleteRoom(int roomNumber);

        Task<bool> UpdateRoomStatus(int roomNumber, State state, IDbTransaction? dbTransaction = null);
    }
}