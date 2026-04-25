using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace Controllers
{
    [ApiController, Route("staff")]
    public class StaffController : Controller
    {
        private IConfiguration Config { get; set; }
        private readonly IReservationRepository _reservationRepo;

        public StaffController(IConfiguration config, IReservationRepository reservationRepo)
        {
            Config = config;
            _reservationRepo = reservationRepo;
        }

        [HttpGet, Route("login")]
        public async Task<IActionResult> CheckCode(
            [FromHeader(Name = "X-Staff-Code")] string accessCode
        )
        {
            Response.Cookies.Delete("access");
            await HttpContext.SignOutAsync("StaffAuth");

            var configuredSecret = Config.GetValue<string>("staffAccessCode");
            if (string.IsNullOrEmpty(configuredSecret) || configuredSecret != accessCode)
            {
                return Unauthorized("Invalid access code.");
            }

            var claims = new List<Claim> { new Claim(ClaimTypes.Role, "Staff") };
            var identity = new ClaimsIdentity(claims, "StaffAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("StaffAuth", principal);

            return Ok("Authenticated");
        }

        [Authorize(AuthenticationSchemes = "StaffAuth")]
        [HttpGet, Route("check")]
        public IActionResult CheckCookie()
        {
            return Ok("Authorized");
        }

        [Authorize(AuthenticationSchemes = "StaffAuth")]
        [HttpGet, Route("reservations")]
        public async Task<IActionResult> GetStaffReservations()
        {
            // Requirement: Today and future reservations
            var reservations = await _reservationRepo.GetUpcomingReservations();
            return Ok(reservations);
        }
    }
}
