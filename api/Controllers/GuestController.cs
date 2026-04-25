using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;
using Repositories.Interfaces;

namespace Controllers
{
    [Tags("Guests"), Route("guest")]
    public class GuestController : Controller
    {
        private IGuestRepository _repo;

        public GuestController(IGuestRepository guestRepository)
        {
            _repo = guestRepository;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Guest>> GetGuests()
        {
            var guests = await _repo.GetGuests();

            return Json(guests);
        }
    }
}
