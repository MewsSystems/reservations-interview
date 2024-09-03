using api.Shared.Models.Domain;

namespace api.Models
{
    public class RoomImportResult
    {
        public required IEnumerable<Room> Success { get; set; }
        public required IEnumerable<ErrorRoomCreateResponse> Fail { get; set; }
    }
}
