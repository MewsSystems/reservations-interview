using api.Shared.Constants;
using api.Shared.Models.Domain;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace api.Shared.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> Get();

        Task<Room> GetByRoomNumber(string roomNumber);

        Task<Room> Create(Room newRoom, IDbConnection? connection = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

        Task<bool> DeleteByRoomNumber(string roomNumber);

        Task<bool> UpdateRoomStatus(string roomNumber, State state, IDbTransaction transaction);

        Task<(IEnumerable<Room> success, IEnumerable<ErrorRoomCreateResponse> fail)> CreateInBatch(Room[] rooms, CancellationToken cancellationToken = default);
    }
}