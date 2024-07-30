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

        /// <summary>
        /// Checks if the request is from a staff member, if not returns true and a 403 result
        /// </summary>
        /// <param name="request"></param>
        private bool IsNotStaff(HttpRequest request, out IActionResult? result)
        {
            // TODO explore UseAuthentication
            request.Cookies.TryGetValue("access", out string? accessValue);

            if (accessValue == null || accessValue == "0")
            {
                result = StatusCode(403);
                return true;
            }

            result = null;
            return false;
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

        [HttpGet, Route("check")]
        public IActionResult CheckCookie()
        {
            if (IsNotStaff(Request, out IActionResult? result))
            {
                return result!;
            }

            return Ok("Authorized");
        }
    }
}
