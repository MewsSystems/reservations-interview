using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Errors;
using Repositories;

namespace Controllers
{
    [Tags("Reservations"), Route("reservation")]
    public class ReservationController : Controller
    {
        private ReservationRepository _repo { get; set; }
        private ILogger<ReservationController> Logger { get; set; }

        public ReservationController(ReservationRepository reservationRepository, ILogger<ReservationController> logger)
        {
            _repo = reservationRepository;
            Logger = logger;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            var reservations = await _repo.GetReservations();

            return Json(reservations);
        }

        [HttpGet, Produces("application/json"), Route("{reservationId}")]
        public async Task<ActionResult<Reservation>> GetRoom(Guid reservationId)
        {
            try
            {
                var reservation = await _repo.GetReservation(reservationId);
                return Json(reservation);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet, Produces("application/json"), Route("room/{roomNumber}")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetRoomReservations(string roomNumber)
        {
            var reservations = await _repo.GetRoomReservations(roomNumber);

            return Json(reservations);
        }

        [HttpGet, Produces("application/json"), Route("upcoming"), Authorize]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetUpcomingReservations()
        {
            var reservations = await _repo.GetUpcomingReservations();
            return Json(reservations);
        }

        /// <summary>
        /// Create a new reservation, to generate the GUID ID on the server, send an Empty GUID (all 0s)
        /// </summary>
        /// <param name="newBooking"></param>
        /// <returns></returns>
        [HttpPost, Produces("application/json"), Route("")]
        public async Task<ActionResult<Reservation>> BookReservation(
            [FromBody] Reservation newBooking
        )
        {
            // Provide a real ID if one is not provided
            if (newBooking.Id == Guid.Empty)
            {
                newBooking.Id = Guid.NewGuid();
            }

            try
            {
                var createdReservation = await _repo.CreateReservation(newBooking);
                return Created($"/reservation/{createdReservation.Id}", createdReservation);
            }
            catch (ReservationConflictException ex)
            {
                Logger.LogWarning(ex, "A reservation conflict occurred when trying to book a reservation");
                return Conflict("Invalid reservation, dates collide with another booking");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred when trying to book a reservation");
                return BadRequest("Invalid reservation");
            }
        }

        [HttpDelete, Produces("application/json"), Route("{reservationId}")]
        public async Task<IActionResult> DeleteReservation(Guid reservationId)
        {
            var result = await _repo.DeleteReservation(reservationId);

            return result ? NoContent() : NotFound();
        }
    }
}
