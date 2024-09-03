using api.Shared.Constants;

namespace api.Shared.Models.DB
{
    public class ReservationWithRoomState : Reservation
    {
        public State State { get; set; }
    }
}
