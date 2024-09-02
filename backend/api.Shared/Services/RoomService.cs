using api.Shared.Constants;
using api.Shared.Extensions;
using api.Shared.Models.Domain;
using api.Shared.Models.Errors;
using api.Shared.Repositories;
using api.Shared.Validation.Domain;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace api.Shared.Services
{
    public class RoomService : IRoomService
    {
        private readonly ILogger<RoomService> _logger;
        private readonly IDbConnection _connection;
        private readonly IRoomRepository _repository;

        public RoomService(
            ILogger<RoomService> logger,
            IDbConnection connection,
            IRoomRepository repository)
        {
            _logger = logger.ThrowIfNull(nameof(ILogger<RoomService>));
            _connection = connection.ThrowIfNull(nameof(IDbConnection));
            _repository = repository.ThrowIfNull(nameof(IRoomRepository));
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
            var roomValidation = await new RoomValidator().ValidateAsync(newRoom);
            if (!roomValidation.IsValid)
            {
                throw new ServiceValidationException(roomValidation.Errors);
            }
            try
            {
                var existingRoom = await _repository.GetRoom(newRoom.FromDomain().Number);
                if (existingRoom != null)
                    throw new RoomAlreadyExistsException(newRoom.Number);
            }
            catch (NotFoundException)
            { }

            var result = (await _repository.CreateRoom(newRoom.FromDomain())).ToDomain();
            _logger?.LogInformation("New room <{@roomNumber}> created.", result.Number);
            return result;
        }

        public async Task<bool> DeleteByRoomNumber(string roomNumber)
        {
            var result = await _repository.DeleteRoom(roomNumber.ConvertRoomNumberToInt());
            if (result)
                _logger?.LogInformation("Room <{roomNumber}> deleted.", roomNumber);
            return result;
        }

        public async Task<bool> UpdateRoomStatus(string roomNumber, State state, IDbTransaction transaction)
        {
            // Check if room exists
            await GetByRoomNumber(roomNumber);
            // Update room status
            var result = await _repository.UpdateRoomStatus(roomNumber.ConvertRoomNumberToInt(), state, transaction);
            if (result)
                _logger?.LogInformation("Room <{roomNumber}> marked as {state}.", roomNumber, state);
            return result;
        }
    }
}
