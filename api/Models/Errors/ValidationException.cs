namespace Models.Errors
{
    public class ValidationException(string resourceType, string resourceId, string message) : ResourceException(resourceType, resourceId, message)
    {
    }
}
