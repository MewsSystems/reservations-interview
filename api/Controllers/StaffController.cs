using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Controllers
{
    [Route("staff")]
    public class StaffController : Controller
    {
        private IConfiguration Config { get; set; }
        private ILogger<StaffController> Logger { get; set; }

        public StaffController(IConfiguration config, ILogger<StaffController> logger)
        {
            Config = config;
            Logger = logger;
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> CheckCode([FromHeader(Name = "X-Staff-Code")] string accessCode)
        {
            var configuredSecret = Config.GetValue<string>("staffAccessCode");
            if (configuredSecret != accessCode)
            {
                Logger.LogWarning("Unauthorised access attempt");
                return Unauthorized();
            }

            var claimsIdentity = new ClaimsIdentity("StaffCookies");
            await HttpContext.SignInAsync("StaffCookies", new ClaimsPrincipal(claimsIdentity));

            return NoContent();
        }

        [HttpGet, Route("check"), Authorize]
        public IActionResult CheckCookie()
        {
            return Ok("Authorized");
        }
    }
}
