using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Errors;
using Repositories;

namespace Controllers
{
    [Tags("Guests"), Route("guest"), Authorize(Policy = "StaffOnly")]
    public class GuestController : Controller
    {
        private GuestRepository _repo;

        public GuestController(GuestRepository guestRepository)
        {
            _repo = guestRepository;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Guest>> GetGuests()
        {
            var guests = await _repo.GetGuests();

            return Json(guests);
        }

        [HttpDelete, Route("{email}")]
        public async Task<ActionResult> DeleteGuest(string email)
        {
            var deleted = await _repo.DeleteGuestByEmail(email);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost, Produces("application/json"), Route("")]
        public async Task<ActionResult<Guest>> CreateGuest([FromBody] Guest? newGuest)
        {
            if (newGuest is null)
            {
                return BadRequest("Invalid guest payload.");
            }

            if (!System.Net.Mail.MailAddress.TryCreate(newGuest.Email, out _))
            {
                return BadRequest("Invalid email address.");
            }

            try
            {
                var created = await _repo.CreateGuest(newGuest);
                var location = $"{Request.PathBase}/guest/{System.Uri.EscapeDataString(created.Email)}";
                return Created(location, created);
            }
            catch (ConflictException)
            {
                return Conflict($"Guest {newGuest.Email} already exists.");
            }
        }

        [HttpPut, Produces("application/json"), Route("{email}")]
        public async Task<ActionResult<Guest>> UpdateGuest(string email, [FromBody] Guest? updatedGuest)
        {
            if (updatedGuest is null)
            {
                return BadRequest("Invalid guest payload.");
            }

            try
            {
                var updated = await _repo.UpdateGuest(email, updatedGuest);
                return Json(updated);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}
