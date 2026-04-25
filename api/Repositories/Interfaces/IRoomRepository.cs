using Models;

namespace Repositories.Interfaces
{
    public interface IRoomRepository
    {
        Task<Room> GetRoom(string roomNumber);
        Task<IEnumerable<Room>> GetRooms();
        Task<Room> CreateRoom(Room newRoom);
        Task<bool> DeleteRoom(string roomNumber);
    }
}
