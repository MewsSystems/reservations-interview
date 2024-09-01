using api.Shared.Models.Domain;
using api.Shared.Models.Errors;
using api.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Tags("Reservations"), Route("reservation")]
    public class ReservationController : Controller
    {
        private IReservationService _service { get; set; }
        public ReservationController(IReservationService service)
        {
            _service = service;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Reservation>> GetReservations()
        {
            var reservations = await _service.Get();
            return Json(reservations);
        }

        [HttpGet, Produces("application/json"), Route("{reservationId}")]
        public async Task<ActionResult<Reservation>> GetRoom(Guid reservationId)
        {
            try
            {
                var reservation = await _service.GetByReservationId(reservationId);
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
            // Provide a real ID if one is not provided
            if (newBooking.Id == Guid.Empty)
            {
                newBooking.Id = Guid.NewGuid();
            }

            try
            {
                var createdReservation = await _service.Create(newBooking);
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
            var result = await _service.Delete(reservationId);

            return result ? NoContent() : NotFound();
        }
    }
}