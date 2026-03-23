namespace Models.Errors
{
    public abstract class ResourceException(Exception? innerException, string resourceType, string resourceId, string message) : Exception(message, innerException)
    {
        public string ResourceType { get; } = resourceType;
        public string ResourceId { get; } = resourceId;


        public ResourceException(string resourceType, string resourceId, string message) : this(null, resourceType, resourceId, message)
        {
        }
    }
}