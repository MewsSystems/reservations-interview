using System.Data;
using Dapper;

public class Guest
{
    /// <summary>
    /// PKID For Guests
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Free form name field to accomodate any and all naming
    /// cultures the guest may have
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// If there is a clear surname, this can be used
    /// in emails/documents instead of the full name
    /// </summary>
    public string? Surname { get; set; }

    /// <summary>
    /// Will return either the Surname, if available, or the whole name.
    /// </summary>
    /// <returns></returns>
    public string GetLastName()
    {
        return Surname ?? Name;
    }

    /// <summary>
    /// Ensures the Guest exists in the db, uses email as name
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public static async Task EnsureGuest(string email, IDbConnection db)
    {
        try
        {
            await db.QuerySingleAsync<Guest>(
                "INSERT INTO Guests(Email, Name) Values(@Email, @Name) RETURNING *",
                // TODO accept a proper name
                // TODO accept an optional surname
                new { Email = email, Name = email }
            );
        }
        catch
        {
            // TODO reimplement this without abusing try-catch
            // guest already exists, everything is OK
        }
    }
}
