using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [Route("staff")]
    public class StaffController : Controller
    {
        private IConfiguration Config { get; set; }

        public StaffController(IConfiguration config)
        {
            Config = config;
        }

        [HttpPost, Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromHeader(Name = "X-Staff-Code")] string accessCode)
        {
            var configuredSecret = Config.GetValue<string>("staffAccessCode");
            if (configuredSecret != accessCode)
            {
                return Unauthorized(new { errors = new[] { "Invalid access code." } });
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, "Staff"),
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return Ok(new { message = "Logged in." });
        }

        [HttpPost, Route("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out." });
        }

        [HttpGet, Route("check")]
        [Authorize]
        public IActionResult CheckAuth()
        {
            return Ok(new { message = "Authorized." });
        }
    }
}
