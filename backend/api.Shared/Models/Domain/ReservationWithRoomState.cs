using api.Shared.Constants;
using System.Text.Json.Serialization;

namespace api.Shared.Models.Domain
{
    public class ReservationWithRoomState : Reservation
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public State State { get; set; }
    }
}
