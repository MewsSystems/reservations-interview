using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [Route("staff")]
    public class StaffController : StaffAccessController
    {
        private IConfiguration Config { get; set; }

        public StaffController(IConfiguration config)
        {
            Config = config;
        }

        [HttpGet, Route("login")]
        public IActionResult CheckCode([FromHeader(Name = "X-Staff-Code")] string accessCode)
        {
            var configuredSecret = Config.GetValue<string>("staffAccessCode");
            if (configuredSecret != accessCode)
            {
                // don't set cookie, don't indicate anything
                return NoContent();
            }
            Response.Cookies.Append(
                StaffAccessCookieName,
                "1",
                new CookieOptions
                // TODO evaluate cookie options & auth mechanism for best security practices
                {
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict,
                    HttpOnly = true,
                    Secure = Request.IsHttps
                }
            );
            return NoContent();
        }

        [HttpGet, Route("check")]
        public IActionResult CheckCookie()
        {
            if (IsNotStaff(Request, out ActionResult? result))
            {
                return result!;
            }

            return Ok("Authorized");
        }
    }
}
