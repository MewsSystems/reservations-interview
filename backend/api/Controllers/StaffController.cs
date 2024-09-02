using api.Authorization;
using api.Models;
using api.Shared.Services;
using api.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace api.Controllers
{
    [Route("staff")]
    public class StaffController : Controller
    {
        private IConfiguration _config;
        private readonly IReservationService _reservationService;

        public StaffController(IConfiguration config, IReservationService reservationService)
        {
            _config = config;
            _reservationService = reservationService;
        }

        [HttpGet, Route("login")]
        public IActionResult CheckCode([FromHeader(Name = "X-Staff-Code")] string accessCode)
        {
            var configuredSecret = _config.GetValue<string>("staffAccessCode");
            var encryptionKey = _config.GetValue<string>("encryptionKey");

            if (string.IsNullOrEmpty(configuredSecret) || string.IsNullOrEmpty(encryptionKey))
                throw new Exception("Staff controller not working. Please set config file with staffAccessCode, encryptionKey");

            if (accessCode != configuredSecret)
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            var encryptedValue = Encryption.AES.Encrypt(
                JsonSerializer.Serialize(new CookieValue()
                {
                    AccessCode = configuredSecret,
                    Ticks = DateTime.Now.Ticks
                }), encryptionKey);

            // Set secure cookie
            Response.Cookies.Append(
                "access",
                encryptedValue,
                new CookieOptions
                {
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict,
                    HttpOnly = false, // We have to use this to use it in javascript
                    Secure = true,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                }
            );

            return NoContent();
        }


        [CookieAuthorization]
        [HttpGet, Route("reservation")]
        public async Task<IActionResult> GetReservations()
        {
            return Json(await _reservationService.GetStaffReservations());
        }

        [CookieAuthorization]
        [HttpGet, Route("check")]
        public IActionResult CheckCookie()
        {
            return Ok("Authorized");
        }
    }
}
