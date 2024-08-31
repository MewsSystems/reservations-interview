using api.Shared.Models;
using api.Shared.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
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
    }
}
