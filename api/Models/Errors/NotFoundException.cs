namespace Models.Errors
{
    public class NotFoundException(string resourceType, string resourceId) : ResourceException(resourceType, resourceId, $"{resourceType} {resourceId} not found")
    {
    }
}
