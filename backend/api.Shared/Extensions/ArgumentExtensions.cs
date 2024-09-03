using System;

namespace api.Shared.Extensions
{
    public static class ArgumentExtensions
    {
        public static T ThrowIfNull<T>(this T value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null.");
            }
            return value;
        }
    }
}
