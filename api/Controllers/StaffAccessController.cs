using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    public abstract class StaffAccessController : Controller
    {
        protected const string StaffAccessCookieName = "access";

        /// <summary>
        /// Checks if the request is from a staff member, if not returns true and a 403 result
        /// </summary>
        protected bool IsNotStaff(HttpRequest request, out ActionResult? result)
        {
            // TODO explore UseAuthentication
            request.Cookies.TryGetValue(StaffAccessCookieName, out string? accessValue);

            if (accessValue == null || accessValue == "0")
            {
                result = StatusCode(403);
                return true;
            }

            result = null;
            return false;
        }
    }
}
