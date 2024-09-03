﻿using api.Models;
using api.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace api.Authorization
{
    public class CookieAuthorizationAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var config = httpContext.RequestServices.GetService<IConfiguration>();

            httpContext.Request.Cookies.TryGetValue("access", out string? accessValue);
            var configuredSecret = config?.GetValue<string>("staffAccessCode");
            var encryptionKey = config?.GetValue<string>("encryptionKey");
            if (accessValue != null && !string.IsNullOrEmpty(encryptionKey) && !string.IsNullOrEmpty(configuredSecret))
            {
                try
                {
                    var decryptedString = Encryption.AES.Decrypt(accessValue, encryptionKey);
                    var cookieValue = JsonSerializer.Deserialize<CookieValue>(decryptedString);
                    if (!string.IsNullOrEmpty(cookieValue?.AccessCode) && cookieValue.AccessCode == configuredSecret)
                    {
                        return;
                    }
                }
                catch
                {

                }
                // If invalid cookie remove it...
                httpContext.Response.Cookies.Delete("access");
            }
            context.Result = new UnauthorizedResult();
        }
    }

}