using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Errors;
using Repositories;

namespace Controllers
{
    [Tags("Reservations"), Route("reservation")]
    public class ReservationController : Controller
    {
        private readonly ReservationRepository _reservationRepository;
        private readonly RoomRepository _roomRepository;
        private readonly GuestRepository _guestRepository;

        public ReservationController(ReservationRepository reservationRepository, RoomRepository roomRepository, GuestRepository guestRepository)
        {
            _reservationRepository = reservationRepository;
            _roomRepository = roomRepository;
            _guestRepository = guestRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            var reservations = await _reservationRepository.GetReservations();

            return Json(reservations);
        }

        [HttpGet("{reservationId}")]
        public async Task<ActionResult<Reservation>> GetReservation(Guid reservationId)
        {
            var reservation = await _reservationRepository.GetReservation(reservationId);
            return Json(reservation);
        }

        /// <summary>
        /// Create a new reservation, to generate the GUID ID on the server, send an Empty GUID (all 0s)
        /// </summary>
        /// <param name="newBooking"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Reservation>> BookReservation([FromBody] Reservation newBooking)
        {
            if (newBooking == null)
                return BadRequest("Request body is required.");

            if (newBooking.Id == Guid.Empty)
                newBooking.Id = Guid.NewGuid();

            if (!await _roomRepository.RoomExists(newBooking.RoomNumber))
                throw new NotFoundException($"Room {newBooking.RoomNumber} does not exist.");

            var guest = await _guestRepository.GetGuestByEmail(newBooking.GuestEmail);

            if (guest == null)
            {
                await _guestRepository.CreateGuest(new Guest
                {
                    Email = newBooking.GuestEmail
                });
            }

            var createdReservation = await _reservationRepository.CreateReservation(newBooking);

            return Created($"/reservation/{createdReservation.Id}", createdReservation);
        }

        [HttpDelete("{reservationId}")]
        public async Task<IActionResult> DeleteReservation(Guid reservationId)
        {
            var result = await _reservationRepository.DeleteReservation(reservationId);

            return result ? NoContent() : NotFound();
        }
    }
}
