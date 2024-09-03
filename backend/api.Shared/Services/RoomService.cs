using api.Shared.Constants;
using api.Shared.Extensions;
using api.Shared.Models.Domain;
using api.Shared.Models.Errors;
using api.Shared.Repositories;
using api.Shared.Services.Core.Factories;
using api.Shared.Validation.Domain;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace api.Shared.Services
{
    public class RoomService : IRoomService
    {
        private readonly ILogger<RoomService> _logger;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IRoomRepository _repository;

        public RoomService(
            ILogger<RoomService> logger,
            IDbConnectionFactory connectionFactory,
            IRoomRepository repository)
        {
            _logger = logger.ThrowIfNull(nameof(ILogger<RoomService>));
            _connectionFactory = connectionFactory.ThrowIfNull(nameof(IDbConnectionFactory));
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

        public async Task<Room> Create(Room newRoom, IDbConnection? connection = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) 
            {
                throw new TaskCanceledException();
            }
            var roomValidation = await new RoomValidator().ValidateAsync(newRoom);
            if (!roomValidation.IsValid)
            {
                throw new ServiceValidationException(roomValidation.Errors);
            }
            try
            {
                var result = (await _repository.CreateRoom(newRoom.FromDomain(), connection, transaction)).ToDomain();
                _logger?.LogDebug("New room <{@roomNumber}> created.", result.Number);
                return result;
            }
            catch(SqliteException e)
            {
                if(e.SqliteErrorCode == 19)
                    throw new RoomAlreadyExistsException(newRoom.Number);
                throw;
            }
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

        public async Task<(IEnumerable<Room> success, IEnumerable<ErrorRoomCreateResponse> fail)> CreateInBatch(Room[] rooms, CancellationToken cancellationToken = default)
        {
            // TODO: Move batching to separate service and make it generic.
            // Overkill for SQLite and not a big performance benefit...
            const int batchSize = 250;
            var tasks = new List<Task<(IEnumerable<Room> success, IEnumerable<ErrorRoomCreateResponse> fail)>>();
            for (int i = 0; i < rooms.Length; i += batchSize)
            {
                var batch = rooms.Skip(i).Take(rooms.Length < batchSize ? rooms.Length : batchSize).ToArray();
                using var connection = _connectionFactory.Get();
                connection.Open();
                _logger.LogInformation("Starting thread for batch import of {roomNumber} rooms...", batch.Length);

                // Bulk insert would be better.
                tasks.Add(
                    Task.Run(() => CreateRooms(batch, connection: connection, cancellationToken: cancellationToken), cancellationToken)
                );
            }
            var result = await Task.WhenAll(tasks);
            var success = result.SelectMany(x => x.success);
            var failed = result.SelectMany(x => x.fail);
            return (success, failed);
        }

        public async Task<(IEnumerable<Room> success, IEnumerable<ErrorRoomCreateResponse> fail)> CreateRooms(
            Room[] rooms, 
            IDbConnection? connection = null, 
            IDbTransaction? transaction = null, 
            CancellationToken cancellationToken = default)
        {
            var success = new List<Room>();
            var fail = new List<ErrorRoomCreateResponse>();
            if (transaction == null && connection != null)
            {
                connection?.Open();
                transaction = connection?.BeginTransaction();
            }
            foreach (var room in rooms)
            {
                try
                {
                    var created = await Create(room, connection, transaction, cancellationToken);
                    if(created != null) success.Add(created);
                    else fail.Add(new() { Room = room, ErrorMessage = "Unexpected error." });
                }
                catch (Exception ex)
                {
                    fail.Add(new() { Room = room, ErrorMessage = ex.Message });
                }
            }
            transaction?.Commit();
            transaction?.Dispose();
            return (success, fail);
        }
    }
}
