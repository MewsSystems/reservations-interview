using api.Shared.Constants;
using api.Shared.Models.Domain;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace api.Shared.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> Get();

        Task<Room> GetByRoomNumber(string roomNumber);

        Task<Room> Create(Room newRoom);

        Task<bool> DeleteByRoomNumber(string roomNumber);

        Task<bool> UpdateRoomStatus(string roomNumber, State state, IDbTransaction transaction);
    }
}