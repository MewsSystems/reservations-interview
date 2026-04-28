using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;

namespace Controllers
{
    [Tags("Staff"), Route("staff")]
    public class StaffController : Controller
    {
        private IConfiguration Config { get; set; }
        private ReservationRepository _reservations { get; set; }
        private IDataProtector _protector { get; set; }

        public StaffController(IConfiguration config, ReservationRepository reservations, IDataProtectionProvider dataProtection)
        {
            Config = config;
            _reservations = reservations;
            _protector = dataProtection.CreateProtector("StaffAccess.v1");
        }

        [HttpGet, Route("login")]
        public IActionResult CheckCode([FromHeader(Name = "X-Staff-Code")] string accessCode)
        {
            var configuredSecret = Config.GetValue<string>("staffAccessCode");

            if (string.IsNullOrEmpty(configuredSecret))
            {
                // staffAccessCode is missing from configuration — this is a server misconfiguration,
                // not a wrong credential. Return 500 so it is distinct from a genuine 403.
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Staff access code is not configured. Contact the system administrator.");
            }

            if (configuredSecret != accessCode)
            {
                return StatusCode(403);
            }

            // Sign the token so the handler can verify it was issued by this server.
            // A client crafting any other cookie value will fail Unprotect() with CryptographicException.
            var token = _protector.Protect("staff-authenticated");

            Response.Cookies.Append(
                "access",
                token,
                new CookieOptions
                {
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict,
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    Path = "/api"
                }
            );
            return NoContent();
        }

        [HttpGet, Produces("application/json"), Route("reservations"), Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> GetReservations()
        {
            var reservations = await _reservations.GetUpcomingReservations();
            return Json(reservations);
        }
    }
}
