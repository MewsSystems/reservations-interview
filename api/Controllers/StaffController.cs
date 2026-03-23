using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Controllers
{
    [ApiController]
    [Route("staff")]
    public class StaffController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StaffController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromHeader(Name = "X-Staff-Code")] string accessCode)
        {
            var configuredSecret = _config.GetValue<string>("staffAccessCode");
            if (accessCode != configuredSecret)
            {
                return Unauthorized(new { detail = "Invalid access code" });
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryHours = _config.GetValue<int>("Jwt:ExpiryHours");

            var token = new JwtSecurityToken(
                claims: [new Claim(ClaimTypes.Role, "Staff")],
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: credentials
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }

}
