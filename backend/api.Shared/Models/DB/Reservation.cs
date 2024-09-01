using System;

namespace api.Shared.Models.DB
{
    public class Reservation
    {
        public required string Id { get; set; }
        
        public int RoomNumber { get; set; }

        public string GuestEmail { get; set; } = "";

        public DateTime Start { get; set; }
        
        public DateTime End { get; set; }
        
        public bool CheckedIn { get; set; }

        public bool CheckedOut { get; set; }
    }
}
