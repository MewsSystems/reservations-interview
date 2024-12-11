using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Controllers
{
    [Route("staff")]
    public class StaffController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IConfiguration config, ILogger<StaffController> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Checks if the request is from a staff member. Returns 403 if not authorized.
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <param name="result">IActionResult to be returned in case of failure</param>
        /// <returns>True if request is unauthorized, false otherwise</returns>
        private bool IsNotStaff(HttpRequest request, out IActionResult? result)
        {
            try
            {
                if (!Request.Cookies.ContainsKey("access") || Request.Cookies["access"] != "1")
                {
                    result = StatusCode(403, "Unauthorized: Staff login is required.");
                    _logger.LogWarning("Unauthorized access attempt from IP {IP} at {Time}", Request.HttpContext.Connection.RemoteIpAddress, DateTime.UtcNow);
                    return true;
                }

                result = null;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking staff access at {Time}", DateTime.UtcNow);
                result = StatusCode(500, "Internal Server Error");
                return true;
            }
        }

        [HttpGet, Route("login")]
        public IActionResult CheckCode([FromHeader(Name = "X-Staff-Code")] string accessCode)
        {
            try
            {
                var configuredSecret = _config.GetValue<string>("StaffAccessCode");

                if (configuredSecret != accessCode)
                {
                    _logger.LogWarning("Invalid access code attempt at {Time}", DateTime.UtcNow);
                    return Unauthorized("Invalid access code.");
                }

                Response.Cookies.Append(
                    "access", 
                    "1", 
                    new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                    });

                _logger.LogInformation("Staff member logged in successfully at {Time}", DateTime.UtcNow);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt at {Time}", DateTime.UtcNow);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet, Route("check")]
        public IActionResult CheckCookie()
        {
            try
            {
                if (IsNotStaff(Request, out IActionResult? result))
                {
                    return result!;
                }

                return Ok("Authorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking staff cookie at {Time}", DateTime.UtcNow);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost, Route("logout")]
        public IActionResult Logout()
        {
            try
            {
                Response.Cookies.Delete("access");

                _logger.LogInformation("Staff member logged out at {Time}", DateTime.UtcNow);

                return Redirect("/login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout at {Time}", DateTime.UtcNow);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
