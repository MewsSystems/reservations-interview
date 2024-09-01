using api.Shared.Models.DB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Shared.Repositories
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetRooms();

        Task<Room> GetRoom(int roomNumber);

        Task<Room> CreateRoom(Room newRoom);

        Task<bool> DeleteRoom(int roomNumber);
    }
}