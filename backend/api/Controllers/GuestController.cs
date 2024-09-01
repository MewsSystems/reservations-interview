using api.Shared.Models.Domain;
using api.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Tags("Guests"), Route("guest")]
    public class GuestController : Controller
    {
        private IGuestService _service;

        public GuestController(IGuestService service)
        {
            _service = service;
        }

        [HttpGet, Produces("application/json"), Route("")]
        public async Task<ActionResult<Guest>> GetGuests()
        {
            var guests = await _service.Get();
            return Json(guests);
        }
    }
}
