using System.Data;
using Dapper;
using Models;
using Models.Errors;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace Repositories
{
    public class ReservationRepository
    {
        private IDbConnection _db { get; set; }
        private RoomRepository _roomRepository { get; set; }
        private GuestRepository _guestRepository { get; set; }

        public ReservationRepository(
            IDbConnection db,
            RoomRepository roomRepository,
            GuestRepository guestRepository
        )
        {
            _db = db;
            _roomRepository = roomRepository;
            _guestRepository = guestRepository;
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

        public async Task<IEnumerable<Reservation>> GetUpcomingReservations()
        {
            var today = DateTime.Today;
            var reservations = await _db.QueryAsync<ReservationDb>(
                @"
                SELECT *
                FROM Reservations
                WHERE End > @today
                ORDER BY Start ASC, RoomNumber ASC;
                ",
                new { today }
            );

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
            newReservation.GuestEmail = newReservation.GuestEmail.Trim();
            newReservation.RoomNumber = newReservation.RoomNumber.Trim();

            ValidateReservation(newReservation);

            await _roomRepository.GetRoom(newReservation.RoomNumber);
            await EnsureNoReservationConflict(newReservation);
            await EnsureGuestExists(newReservation.GuestEmail);

            var createdReservation = await _db.QuerySingleAsync<ReservationDb>(
                @"
                INSERT INTO Reservations(
                    Id,
                    GuestEmail,
                    RoomNumber,
                    Start,
                    End,
                    CheckedIn,
                    CheckedOut
                )
                VALUES(
                    @Id,
                    @GuestEmail,
                    @RoomNumber,
                    @Start,
                    @End,
                    @CheckedIn,
                    @CheckedOut
                )
                RETURNING *;
                ",
                new ReservationDb(newReservation)
            );

            return createdReservation.ToDomain();
        }

        public async Task<bool> DeleteReservation(Guid reservationId)
        {
            var deleted = await _db.ExecuteAsync(
                "DELETE FROM Reservations WHERE Id = @reservationIdStr;",
                new { reservationIdStr = reservationId.ToString() }
            );

            return deleted > 0;
        }

        public async Task<Reservation> CheckInReservation(
            Guid reservationId,
            string guestEmailConfirmation
        )
        {
            var reservation = await GetReservation(reservationId);
            var trimmedGuestEmail = guestEmailConfirmation?.Trim() ?? "";

            ValidateCheckIn(reservation, trimmedGuestEmail);

            if (_db is SqliteConnection sqliteConnection && sqliteConnection.State != ConnectionState.Open)
            {
                await sqliteConnection.OpenAsync();
            }

            using var transaction = _db.BeginTransaction();

            try
            {
                await _db.ExecuteAsync(
                    "UPDATE Reservations SET CheckedIn = TRUE WHERE Id = @reservationIdStr;",
                    new { reservationIdStr = reservationId.ToString() },
                    transaction
                );

                await _db.ExecuteAsync(
                    "UPDATE Rooms SET State = @occupiedState WHERE Number = @roomNumber;",
                    new
                    {
                        occupiedState = State.Occupied,
                        roomNumber = Room.ConvertRoomNumberToInt(reservation.RoomNumber)
                    },
                    transaction
                );

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return await GetReservation(reservationId);
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

        private static void ValidateReservation(Reservation reservation)
        {
            if (!Room.IsValidRoomNumber(reservation.RoomNumber))
            {
                throw new InvalidReservationException("Room number must use the ### format.");
            }

            if (!LooksLikeEmailWithDomain(reservation.GuestEmail))
            {
                throw new InvalidReservationException("Email must include a domain.");
            }

            if (reservation.Start >= reservation.End)
            {
                throw new InvalidReservationException("Start date must be before the end date.");
            }

            var duration = reservation.End - reservation.Start;
            if (duration < TimeSpan.FromDays(1))
            {
                throw new InvalidReservationException("Reservation duration must be at least 1 day.");
            }

            if (duration > TimeSpan.FromDays(30))
            {
                throw new InvalidReservationException("Reservation duration cannot exceed 30 days.");
            }
        }

        private static void ValidateCheckIn(Reservation reservation, string guestEmailConfirmation)
        {
            if (!string.Equals(
                    reservation.GuestEmail,
                    guestEmailConfirmation,
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                throw new InvalidCheckInException(
                    "Guest email confirmation does not match the reservation."
                );
            }

            if (reservation.CheckedIn)
            {
                throw new InvalidCheckInException("Reservation is already checked in.");
            }

            if (reservation.CheckedOut)
            {
                throw new InvalidCheckInException("Checked out reservations cannot be checked in.");
            }

            if (reservation.Start.Date != DateTime.Today)
            {
                throw new InvalidCheckInException(
                    "Only reservations starting today can be checked in."
                );
            }
        }

        private static bool LooksLikeEmailWithDomain(string guestEmail)
        {
            var trimmedEmail = guestEmail.Trim();
            var parts = trimmedEmail.Split('@', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return false;
            }

            return parts[1].Contains('.') && parts[1].Length > 2;
        }

        private async Task EnsureGuestExists(string guestEmail)
        {
            try
            {
                await _guestRepository.GetGuestByEmail(guestEmail);
            }
            catch (NotFoundException)
            {
                await _guestRepository.CreateGuest(
                    new Guest { Email = guestEmail, Name = BuildGuestName(guestEmail) }
                );
            }
        }

        private async Task EnsureNoReservationConflict(Reservation newReservation)
        {
            var roomNumber = Room.ConvertRoomNumberToInt(newReservation.RoomNumber);
            var conflictingReservation = await _db.QueryFirstOrDefaultAsync<string>(
                @"
                SELECT Id
                FROM Reservations
                WHERE RoomNumber = @roomNumber
                  AND Start < @reservationEnd
                  AND End > @reservationStart
                LIMIT 1;
                ",
                new
                {
                    roomNumber,
                    reservationStart = newReservation.Start,
                    reservationEnd = newReservation.End
                }
            );

            if (!string.IsNullOrEmpty(conflictingReservation))
            {
                throw new ReservationConflictException(
                    $"Room {newReservation.RoomNumber} is already booked for the selected dates."
                );
            }
        }

        private static string BuildGuestName(string guestEmail)
        {
            var localPart = guestEmail.Split('@', 2)[0];
            var spacedName = localPart.Replace('.', ' ').Replace('_', ' ').Replace('-', ' ');
            var candidateName = spacedName.Trim();

            if (string.IsNullOrWhiteSpace(candidateName))
            {
                return "Guest";
            }

            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(candidateName);
        }
    }
}
