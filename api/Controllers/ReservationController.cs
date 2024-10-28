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
        private RoomRepository _roomRepo { get; set; }
        private GuestRepository _guestRepo { get; set; }

        public static int MAX_DAYS = 30;

        public ReservationController(ReservationRepository reservationRepository, RoomRepository roomRepo, GuestRepository guestRepo)
        {
            _repo = reservationRepository;
            _roomRepo = roomRepo;
            _guestRepo = guestRepo;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Reservation>> GetReservations()
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
            if (newBooking.Start >= newBooking.End)
            {
                ModelState.AddModelError(nameof(newBooking.End), "End date must be after start date.");
            }

            var duration = newBooking.End - newBooking.Start;
            if (duration.TotalDays > MAX_DAYS)
            {
                ModelState.AddModelError(nameof(newBooking.End), $"Reservations cannot be longer than {MAX_DAYS} days.");
            }

            // TODO: consider validating that the dates are in future

            if (!Room.IsValidRoomNumberString(newBooking.RoomNumber))
            {
                ModelState.AddModelError(nameof(newBooking.RoomNumber), $"Invalid room number.");
            }

            try
            { 
                await _roomRepo.GetRoom(newBooking.RoomNumber); 
            }
            catch (NotFoundException)
            {
                ModelState.AddModelError(nameof(newBooking.RoomNumber), $"Room doesn't exist.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _guestRepo.GetGuestByEmail(newBooking.GuestEmail);
            }
            catch (NotFoundException)
            {
                await _guestRepo.CreateGuest(new Guest { Email = newBooking.GuestEmail, Name = "" });
            }

            // Provide a real ID if one is not provided
            if (newBooking.Id == Guid.Empty)
            {
                newBooking.Id = Guid.NewGuid();
            }

            try
            {
                var createdReservation = await _repo.CreateReservation(newBooking);
                return Created($"/reservation/${createdReservation.Id}", createdReservation);
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
            var result = await _repo.DeleteReservation(reservationId);

            return result ? NoContent() : NotFound();
        }
    }
}
