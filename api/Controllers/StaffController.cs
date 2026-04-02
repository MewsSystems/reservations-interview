using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;

namespace Controllers
{
    [Route("staff")]
    public class StaffController : Controller
    {
        private readonly ReservationRepository _reservationRepository;
        private readonly IConfiguration _сonfig;

        public StaffController(ReservationRepository reservationRepository, IConfiguration config)
        {
            _reservationRepository = reservationRepository;
            _сonfig = config;
        }

        /// <summary>
        /// Checks if the request is from a staff member, if not returns true and a 403 result
        /// </summary>
        /// <param name="request"></param>
        private bool IsNotStaff(HttpRequest request, out ActionResult? result)
        {
            // TODO explore UseAuthentication
            request.Cookies.TryGetValue("access", out string? accessValue);

            if (accessValue != "1")
            {
                result = Unauthorized();
                return true;
            }

            result = null;
            return false;
        }

        [HttpPost("login")]
        public IActionResult Login([FromHeader(Name = "X-Staff-Code")] string accessCode)
        {
            var configuredSecret = _сonfig.GetValue<string>("staffAccessCode");

            if (string.IsNullOrWhiteSpace(accessCode) || configuredSecret != accessCode)
            {
                return Unauthorized();
            }

            Response.Cookies.Append(
                "access",
                "1",
                new CookieOptions
                // TODO evaluate cookie options & auth mechanism for best security practices
                {
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict,
                    HttpOnly = true,
                    Secure = true
                }
            );

            return NoContent();
        }

        [HttpGet("check")]
        public IActionResult CheckCookie()
        {
            if (IsNotStaff(Request, out ActionResult? result))
            {
                return result!;
            }

            return Ok("Authorized");
        }

        [HttpGet("reservations")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetUpcomingReservations()
        {
            try
            {
                if (IsNotStaff(Request, out ActionResult? result))
                {
                    return result!;
                }

                var reservations = await _reservationRepository.GetTodayAndUpcomingReservations();

                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
