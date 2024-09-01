using api.Shared.Extensions;
using api.Shared.Models.Domain;
using api.Shared.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Shared.Services
{
    public class RoomService : IRoomService
    {
        private readonly ILogger<RoomService> _logger;
        private readonly IRoomRepository _repository;

        public RoomService(ILogger<RoomService> logger, IRoomRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<IEnumerable<Room>> Get()
        {
            return (await _repository.GetRooms()).ToDomain();
        }

        public async Task<Room> GetByRoomNumber(string roomNumber)
        {
            return (await _repository.GetRoom(roomNumber.ConvertRoomNumberToInt())).ToDomain();
        }

        public async Task<Room> Create(Room newRoom)
        {
            var result = (await _repository.CreateRoom(newRoom.FromDomain())).ToDomain();
            _logger?.LogInformation("New room <{@roomNumber}> created.", result.Number);
            return result;
        }

        public async Task<bool> DeleteByRoomNumber(string roomNumber)
        {
            var result = await _repository.DeleteRoom(roomNumber.ConvertRoomNumberToInt());
            if (result)
                _logger?.LogInformation("Room <{@roomNumber}> deleted.", roomNumber);
            return result;
        }
    }
}
