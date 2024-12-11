using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;

namespace Controllers
{
    [Tags("Guests"), Route("guest")]
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

        /// <summary>
        /// Create a new guest
        /// </summary>
        /// <param name="newGuest"></param>
        /// <returns></returns>
        [HttpPost, Produces("application/json"), Route("")]
        public async Task<ActionResult<Reservation>> CreateGuest(
            [FromBody] Guest newGuest
        )
        {
            await _repo.CreateGuest(newGuest);
            return Created();
        }
    }
}
