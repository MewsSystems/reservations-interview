using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;

namespace Controllers
{
    [ApiController]
    [Tags("Guests"), Route("guests")]
    public class GuestsController : ControllerBase
    {
        private GuestRepository _repo;

        public GuestsController(GuestRepository guestRepository)
        {
            _repo = guestRepository;
        }

        [HttpGet, Produces("application/json"), Route("")]
        [ProducesResponseType(typeof(IEnumerable<Guest>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Guest>> GetGuests()
        {
            var guests = await _repo.GetGuests();

            return Ok(guests);
        }
    }
}
