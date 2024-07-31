using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Controllers
{
    [Tags("Reservations"), Route("reservation")]
    public class ReservationController : Controller
    {
        private SqliteConnection db { get; set; }

        public ReservationController(SqliteConnection sqliteDb)
        {
            db = sqliteDb;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Reservation>> GetReservations()
        {
            var reservations = await db.QueryAsync<DbReservation>("SELECT * FROM Reservations;");

            if (reservations == null)
            {
                return Json(Enumerable.Empty<Reservation>());
            }

            return Json(reservations.Select(r => r.ToDomain()));
        }

        [HttpGet, Produces("application/json"), Route("{reservationId}")]
        public async Task<ActionResult<Reservation>> GetRoom(Guid reservationId)
        {
            var reservation = await db.QueryFirstOrDefaultAsync<DbReservation>(
                "SELECT * FROM Reservations WHERE Id = @reservationIdStr;",
                new { reservationIdStr = reservationId.ToString() }
            );

            if (reservation == null)
            {
                return NotFound();
            }

            return Json(reservation.ToDomain());
        }

        [HttpPost, Produces("application/json"), Route("")]
        public async Task<ActionResult<Reservation>> BookReservation(NewReservation newBooking)
        {
            var newReservation = new Reservation
            {
                Id = Guid.NewGuid(),
                GuestEmail = newBooking.GuestEmail,
                RoomNumber = newBooking.RoomNumber,
                Start = newBooking.Start,
                End = newBooking.End
            };

            try
            {
                // TODO FK?
                // TODO var persistedReservation = ...
                await Task.CompletedTask;

                // TODO change to persisted
                return Created($"/reservation/${newReservation.Id}", newReservation);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured when trying to book a reservation:");
                Console.WriteLine(ex.ToString());

                return BadRequest("Invalid reservation");
            }
        }

        [HttpDelete, Produces("application/json"), Route("{reservationId}")]
        public async Task<IActionResult> DeleteReservation(Guid reservationId)
        {
            var result = await db.QuerySingleOrDefaultAsync(
                "DELETE FROM Reservations WHERE Id = @reservationIdStr;",
                new { reservationIdStr = reservationId.ToString() }
            );

            return result == 1 ? NotFound() : NoContent();
        }
    }
}
