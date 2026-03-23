using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Contracts;
using FluentValidation;
using Models;
using Repositories;

namespace Controllers
{
    [ApiController]
    [Tags("Reservations"), Route("reservations")]
    public class ReservationsController : ControllerBase
    {
        private ReservationRepository _repo { get; set; }
        private IValidator<ReservationRequest> _validator { get; set; }

        public ReservationsController(ReservationRepository reservationRepository, IValidator<ReservationRequest> validator)
        {
            _repo = reservationRepository;
            _validator = validator;
        }

        [Authorize]
        [HttpGet, Produces("application/json"), Route("")]
        [ProducesResponseType(typeof(IEnumerable<Reservation>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? roomNumber,
            [FromQuery] string? guestEmail)
        {
            var reservations = await _repo.GetReservations(from, to, roomNumber, guestEmail);

            return Ok(reservations);
        }

        [HttpGet, Produces("application/json"), Route("{reservationId}")]
        [ProducesResponseType(typeof(Reservation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Reservation>> GetReservation(Guid reservationId)
        {
            var reservation = await _repo.GetReservation(reservationId);
            
            return Ok(reservation);
        }

        /// <summary>
        /// Create a new reservation, to generate the GUID ID on the server, send an Empty GUID (all 0s)
        /// </summary>
        /// <param name="request"> The request/booking data. </param>
        /// <returns> The created reservation. </returns>
        [HttpPost, Produces("application/json"), Route("")]
        [ProducesResponseType(typeof(Reservation), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Reservation>> BookReservation([FromBody] ReservationRequest request)
        {
            await _validator.ValidateAndThrowAsync(request);

            var createdReservation = await _repo.CreateReservation(request);
            return Created($"/reservations/{createdReservation.Id}", createdReservation);
        }

        [Authorize]
        [HttpPost, Produces("application/json"), Route("{reservationId}/check-in")]
        [ProducesResponseType(typeof(Reservation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Reservation>> CheckIn(Guid reservationId, [FromBody] CheckInRequest request)
        {
            var reservation = await _repo.CheckIn(reservationId, request.GuestEmail);
            return Ok(reservation);
        }

        [Authorize]
        [HttpPost, Produces("application/json"), Route("{reservationId}/check-out")]
        [ProducesResponseType(typeof(Reservation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Reservation>> CheckOut(Guid reservationId, [FromBody] CheckInRequest request)
        {
            var reservation = await _repo.CheckOut(reservationId, request.GuestEmail);
            return Ok(reservation);
        }

        [Authorize]
        [HttpDelete, Produces("application/json"), Route("{reservationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReservation(Guid reservationId)
        {
            await _repo.DeleteReservation(reservationId);

            return NoContent();
        }
    }
}
