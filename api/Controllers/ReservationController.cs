using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Errors;
using Models.Validators;
using Repositories;

namespace Controllers
{
    [Tags("Reservations"), Route("reservation")]
    public class ReservationController : Controller
    {
        private ReservationRepository _repo { get; set; }
        private readonly ReservationValidator _validator;
        private readonly RoomValidator _roomValidator;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(ReservationRepository reservationRepository, ReservationValidator reservationValidator, RoomValidator roomValidator, ILogger<ReservationController> logger)
        {
            _repo = reservationRepository;
            _validator = reservationValidator;
            _roomValidator = roomValidator;
            _logger = logger;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Reservation>> GetReservations()
        {
            try
            {
                var reservations = await _repo.GetReservations();
                _logger.LogInformation("Successfully fetched {ReservationCount} reservations.", reservations.Count());
                return Json(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching reservations.");
                return StatusCode(500, "An error occurred while fetching the reservations.");
            }
        }

        [HttpGet, Produces("application/json"), Route("{reservationId}")]
        public async Task<ActionResult<Reservation>> GetRoom(Guid reservationId)
        {
            try
            {
                var reservation = await _repo.GetReservation(reservationId);
                _logger.LogInformation("Successfully fetched reservation with ID {ReservationId}.", reservationId);
                return Json(reservation);
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("Reservation with ID {ReservationId} not found.", reservationId);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the reservation with ID {ReservationId}.", reservationId);
                return StatusCode(500, "An error occurred while fetching the reservation.");
            }
        }

        /// <summary>
        /// Create a new reservation, to generate the GUID ID on the server, send an Empty GUID (all 0s)
        /// </summary>
        /// <param name="newBooking"></param>
        /// <returns></returns>
        [HttpPost, Produces("application/json"), Route("")]
        public async Task<ActionResult<Reservation>> BookReservation([FromBody] Reservation newBooking)


        {
            var validationResult = _validator.ValidateReservation(newBooking);
            if (!validationResult.IsValid)
            {
                 _logger.LogWarning("Invalid reservation: {ErrorMessage}", validationResult.ErrorMessage);
                return BadRequest(validationResult.ErrorMessage);
            }

            if (!_roomValidator.IsValidRoomNumber(newBooking.RoomNumber))
            {
                _logger.LogWarning("Invalid room number provided: {RoomNumber}", newBooking.RoomNumber);
                return BadRequest("Invalid room number. Ensure it follows the proper format and rules.");
            }

            try
            {
                if (newBooking.Id == Guid.Empty)
                {
                    newBooking.Id = Guid.NewGuid();
                }

                var createdReservation = await _repo.CreateReservation(newBooking);
                _logger.LogInformation("Successfully created a reservation with ID {ReservationId}.", createdReservation.Id);
                return Created($"/reservation/{createdReservation.Id}", createdReservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to book a reservation.");
                return BadRequest("An error occurred while processing the reservation.");


            }
        }

        [HttpDelete, Produces("application/json"), Route("{reservationId}")]
        public async Task<IActionResult> DeleteReservation(Guid reservationId)
        {
            try
            {
                var result = await _repo.DeleteReservation(reservationId);
                if (result)
                {
                    _logger.LogInformation("Reservation with ID {ReservationId} successfully deleted.", reservationId);
                    return NoContent();
                }
                else
                {
                    _logger.LogWarning("Reservation with ID {ReservationId} not found for deletion.", reservationId);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to delete reservation with ID {ReservationId}.", reservationId);
                return StatusCode(500, "An error occurred while deleting the reservation.");
            }
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingReservations()
        {
            try
            {
                var upcomingReservations = await _repo.GetUpcomingReservations();

                if (upcomingReservations == null || !upcomingReservations.Any())
                {
                    return NoContent();
                }

                return Ok(upcomingReservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}

