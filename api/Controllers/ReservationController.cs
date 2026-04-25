using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Errors;
using Repositories;
using Repositories.Interfaces;
using Validators;
using Validators.Interfaces;

namespace Controllers
{
    [Tags("Reservations"), Route("reservation")]
    [ApiController]
    public class ReservationController : Controller
    {
        private readonly IReservationRepository _reservationRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly IGuestRepository _guestRepo;
        private readonly IReservationValidator _reservationValidator;

        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled
        );
        private static readonly Regex RoomNumberRegex = new Regex(
            @"^[0-9](0[1-9]|[1-9][0-9])$",
            RegexOptions.Compiled
        );
        private static readonly string[] error = new[] { "Request payload is missing or invalid." };

        public ReservationController(
            IReservationRepository reservationRepository,
            IRoomRepository roomRepository,
            IGuestRepository guestRepository,
            IReservationValidator reservationValidator
        )
        {
            _reservationRepo = reservationRepository;
            _roomRepo = roomRepository;
            _guestRepo = guestRepository;
            _reservationValidator = reservationValidator;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Reservation>> GetReservations()
        {
            var reservations = await _reservationRepo.GetReservations();

            return Json(reservations);
        }

        [HttpGet, Produces("application/json"), Route("{reservationId}")]
        public async Task<ActionResult<Reservation>> GetRoom(Guid reservationId)
        {
            try
            {
                var reservation = await _reservationRepo.GetReservation(reservationId);
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
            if (newBooking == null)
            {
                return BadRequest(new { errors = error });
            }

            var validationErrors = _reservationValidator.Validate(newBooking);
            if (validationErrors.Any())
            {
                return BadRequest(new { errors = validationErrors });
            }

            // Verify room exists
            try
            {
                await _roomRepo.GetRoom(newBooking.RoomNumber);
            }
            catch (NotFoundException)
            {
                return BadRequest(new { errors = new[] { "Room does not exist." } });
            }

            try
            {
                await _guestRepo.GetGuestByEmail(newBooking.GuestEmail);
            }
            catch (NotFoundException)
            {
                await _guestRepo.CreateGuest(
                    new Guest
                    {
                        Email = newBooking.GuestEmail,
                        Name = string.Empty,
                    }
                );
            }

            // Provide a real ID if one is not provided
            if (newBooking.Id == Guid.Empty)
            {
                newBooking.Id = Guid.NewGuid();
            }

            try
            {
                var createdReservation = await _reservationRepo.CreateReservation(newBooking);
                return Created($"/reservation/{createdReservation.Id}", createdReservation);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { errors = new[] { ex.Message } });
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
            var result = await _reservationRepo.DeleteReservation(reservationId);

            return result ? NoContent() : NotFound();
        }
    }
}
