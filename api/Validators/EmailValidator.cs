using System.Net.Mail;

namespace Validators;

public static class EmailValidator
{
    public static void Validate(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Guest email is required.");

        if (!IsValid(email))
            throw new ArgumentException("Guest email is invalid.");
    }

    public static bool IsValid(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email);

            var domain = email.Split('@').Last();
            return mailAddress.Address == email && domain.Contains('.');
        }
        catch
        {
            return false;
        }
    }
}