using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Authorization
{
    public class StaffRequirement : IAuthorizationRequirement { }

    public class StaffAuthorizationHandler : AuthorizationHandler<StaffRequirement>
    {
        private readonly IDataProtector _protector;

        public StaffAuthorizationHandler(IDataProtectionProvider dataProtection)
        {
            _protector = dataProtection.CreateProtector("StaffAccess.v1");
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            StaffRequirement requirement)
        {
            var httpContext = (context.Resource as AuthorizationFilterContext)?.HttpContext;
            if (httpContext != null &&
                httpContext.Request.Cookies.TryGetValue("access", out string? token) &&
                token != null)
            {
                try
                {
                    // Throws CryptographicException if the value was tampered with or forged.
                    var payload = _protector.Unprotect(token);
                    if (payload == "staff-authenticated")
                    {
                        context.Succeed(requirement);
                    }
                }
                catch
                {
                    // Invalid or forged token — leave requirement unsatisfied.
                }
            }

            return Task.CompletedTask;
        }
    }
}
