using api.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Errors;
using Repositories;

namespace Controllers
{
    [Tags("Reservations"), Route("reservation")]
    public class ReservationController : Controller
    {
        private readonly IReservationValidation _reservationValidation;
        private readonly RoomRepository _roomRepository;
        private ReservationRepository _reservationRepository;

        public ReservationController(ReservationRepository reservationRepository, IReservationValidation reservationValidation, RoomRepository roomRepository)
        {
            _reservationValidation = reservationValidation;
            _roomRepository = roomRepository;
            _reservationRepository = reservationRepository;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Reservation>> GetReservations()
        {
            var reservations = await _reservationRepository.GetReservations();

            return Json(reservations);
        }

        [HttpGet, Produces("application/json"), Route("{reservationId}")]
        public async Task<ActionResult<Reservation>> GetRoom(Guid reservationId)
        {
            try
            {
                var reservation = await _reservationRepository.GetReservation(reservationId);
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
            // Validate booking
            var validationResult = _reservationValidation.ValidateReservation(newBooking);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return BadRequest(validationResult);
            }

            // Provide a real ID if one is not provided
            if (newBooking.Id == Guid.Empty)
            {
                newBooking.Id = Guid.NewGuid();
            }

            try
            {
                // GetRoom will throw in case room was not found
                await _roomRepository.GetRoom(newBooking.RoomNumber);

                var createdReservation = await _reservationRepository.CreateReservation(newBooking);
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
            var result = await _reservationRepository.DeleteReservation(reservationId);

            return result ? NoContent() : NotFound();
        }
    }
}
