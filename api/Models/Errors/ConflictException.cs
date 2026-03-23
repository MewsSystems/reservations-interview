namespace Models.Errors
{
    public class ConflictException(string resourceType, string resourceId, string message) : ResourceException(resourceType, resourceId, message)
    {
    }
}
