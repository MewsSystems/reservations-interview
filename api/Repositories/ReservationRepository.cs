using System.Data;
using System.Net.Mail;
using Dapper;
using Models;
using Models.Errors;

namespace Repositories
{
    public class ReservationRepository
    {
        private IDbConnection _db { get; set; }

        public ReservationRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Reservation>> GetReservations()
        {
            var reservations = await _db.QueryAsync<ReservationDb>("SELECT * FROM Reservations");

            if (reservations == null)
            {
                return [];
            }

            return reservations.Select(r => r.ToDomain());
        }

        /// <summary>
        /// Find a reservation by its Guid ID, throwing if not found
        /// </summary>
        /// <param name="reservationId"></param>
        /// <returns cref="Reservation">An existing reservation</returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<Reservation> GetReservation(Guid reservationId)
        {
            var reservation = await _db.QueryFirstOrDefaultAsync<ReservationDb>(
                "SELECT * FROM Reservations WHERE Id = @reservationIdStr;",
                new { reservationIdStr = reservationId.ToString() }
            );

            if (reservation == null)
            {
                throw new NotFoundException($"Room {reservationId} not found");
            }

            return reservation.ToDomain();
        }

        public async Task<Reservation> CreateReservation(Reservation newReservation)
        {
            ValidateReservation(newReservation);

            var roomNumberInt = Room.ConvertRoomNumberToInt(newReservation.RoomNumber);
            var roomExists = await _db.QueryFirstOrDefaultAsync<int?>("SELECT Number FROM Rooms WHERE Number = @roomNumberInt;", new { roomNumberInt });

            if (roomExists == null)
                throw new ReservationValidationException($"Room {newReservation.RoomNumber} does not exist.");

            var normalizedReservation = new Reservation
            {
                Id = newReservation.Id,
                RoomNumber = newReservation.RoomNumber,
                GuestEmail = newReservation.GuestEmail.Trim(),
                Start = newReservation.Start,
                End = newReservation.End,
                CheckedIn = newReservation.CheckedIn,
                CheckedOut = newReservation.CheckedOut
            };

            var hasConflict = await _db.QueryFirstOrDefaultAsync<int?>(
                @"
                SELECT 1
                FROM Reservations
                WHERE RoomNumber = @RoomNumber
                  AND Start < @End
                  AND @Start < End
                LIMIT 1;
                ",
                new
                {
                    RoomNumber = roomNumberInt,
                    normalizedReservation.Start,
                    normalizedReservation.End
                }
            );

            if (hasConflict != null)
                throw new ReservationValidationException(
                    $"Room {normalizedReservation.RoomNumber} is already reserved for the selected dates."
                );

            // TODO3HRS: better to have reservation service, not to call directly (not for 3 hrs)
            await _db.ExecuteAsync(@"INSERT INTO Guests(Email, Name) VALUES(@Email, @Name) ON CONFLICT(Email) DO NOTHING;",
                new
                {
                    Email = normalizedReservation.GuestEmail,
                    Name = normalizedReservation.GuestEmail
                }
            );

            var createdReservation = await _db.QuerySingleAsync<ReservationDb>(
                @"
                INSERT INTO Reservations(Id, GuestEmail, RoomNumber, Start, End, CheckedIn, CheckedOut)
                VALUES(@Id, @GuestEmail, @RoomNumber, @Start, @End, @CheckedIn, @CheckedOut)
                RETURNING *;
                ",
                new ReservationDb(normalizedReservation)
            );

            return createdReservation.ToDomain();
        }

        //TODO3HRS: extract to some validator (not for 3 hrs)
        //TODODISCUSSION: Is not written in RE-001, but I would add backward validation.. 
        private static void ValidateReservation(Reservation reservation)
        {
            if (reservation.Id == Guid.Empty)
                throw new ReservationValidationException("Reservation ID is required.");

            if (reservation.Start >= reservation.End)
                throw new ReservationValidationException("Start date must be before end date.");

            var duration = reservation.End - reservation.Start;
            if (duration.TotalDays < 1)
                throw new ReservationValidationException("Minimum stay is 1 day.");

            if (duration.TotalDays > 30)
                throw new ReservationValidationException("Maximum stay is 30 days.");

            if (!IsEmail(reservation.GuestEmail))
                throw new ReservationValidationException("Guest email must be valid.");
        }

        //TODO3HRS: its better tohave strict API boundary validation: DTO + data annotations like [EmailAddress] (not for 3 hrs)
        private static bool IsEmail(string email)
        {
            try
            {
                var trimmedEmail = email.Trim();
                var parsedEmail = new MailAddress(trimmedEmail);
                return parsedEmail.Address == trimmedEmail && parsedEmail.Host.Contains('.');
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteReservation(Guid reservationId)
        {
            var deleted = await _db.ExecuteAsync(
                "DELETE FROM Reservations WHERE Id = @reservationIdStr;",
                new { reservationIdStr = reservationId.ToString() }
            );

            return deleted > 0;
        }

        private class ReservationDb
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
}
