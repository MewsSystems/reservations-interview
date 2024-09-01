using System;

namespace api.Shared.Models.Domain
{
    public class Reservation
    {
        /// <summary>
        /// PKID for Reservations
        /// </summary>
        public Guid Id { get; set; }
    
        public required string RoomNumber { get; set; }

        public required string GuestEmail { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool CheckedIn { get; set; }
        public bool CheckedOut { get; set; }
    }
}
