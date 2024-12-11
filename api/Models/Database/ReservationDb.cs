namespace Models.Database
{
    public class ReservationDb
    {
        public string Id { get; set; }
        public int RoomNumber { get; set; }

        public string GuestEmail { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool CheckedIn { get; set; }
        public bool CheckedOut { get; set; }

        public ReservationDb()
        {
            Id = Guid.Empty.ToString();
            RoomNumber = 0;
            GuestEmail = "";
        }

        public ReservationDb(Reservation reservation)
        {
            Id = reservation.Id.ToString();
            RoomNumber = Room.ConvertRoomNumberToInt(reservation.RoomNumber);
            GuestEmail = reservation.GuestEmail;
            Start = reservation.Start;
            End = reservation.End;
            CheckedIn = reservation.CheckedIn;
            CheckedOut = reservation.CheckedOut;
        }

        public Reservation ToDomain()
        {
            return new Reservation
            {
                Id = Guid.Parse(Id),
                RoomNumber = Room.FormatRoomNumber(RoomNumber),
                GuestEmail = GuestEmail,
                Start = Start,
                End = End,
                CheckedIn = CheckedIn,
                CheckedOut = CheckedOut
            };
        }
    }
}

