using System;

namespace api.Shared.Models.Errors
{
    public class ReservationUnavailableException : Exception
    {
        public ReservationUnavailableException(int roomNumber, DateTime start, DateTime end)
            : base($"Room {roomNumber} is no longer available for reservation on dates start:{start}, end:{end}.") { }
    }
}
