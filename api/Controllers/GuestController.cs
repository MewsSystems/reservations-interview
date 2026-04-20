using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Errors;
using Repositories;

namespace Controllers
{
    [Tags("Guests"), Route("guest")]
    public class GuestController : Controller
    {
        private GuestRepository _repo;
        private ILogger<GuestController> Logger { get; set; }

        public GuestController(GuestRepository guestRepository, ILogger<GuestController> logger)
        {
            _repo = guestRepository;
            Logger = logger;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Guest>> GetGuests()
        {
            var guests = await _repo.GetGuests();

            return Json(guests);
        }

        [HttpPost, Produces("application/json"), Route("")]
        public async Task<ActionResult<Guest>> AddGuest([FromBody] Guest guest)
        {
            try
            {
                var registeredGuest = await _repo.CreateGuest(guest);
                return Created($"/guest/{registeredGuest.Email}", registeredGuest);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred when trying to register a new guest");

                return BadRequest("Invalid guest data");
            }
        }   
    }
}
