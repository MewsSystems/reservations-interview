using api.Authorization;
using api.Models;
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

        [HttpGet, Produces("application/json"), Route("{userEmail}")]
        public async Task<ActionResult<Guest>> GetGuest(string userEmail)
        {
            var user = await _service.GetByEmail(userEmail);
            return Json(user);
        }

        [CookieAuthorization]
        [HttpPatch, Produces("application/json"), Route("")]
        public async Task<ActionResult<bool>> ConfirmAccount([FromBody] AccountConfirmationRequest request)
        {
            if (string.IsNullOrEmpty(request?.GuestEmail))
                return BadRequest();

            var result = await _service.ConfirmAccount(request.GuestEmail);
            return Json(new { Success = result });
        }
    }
}
