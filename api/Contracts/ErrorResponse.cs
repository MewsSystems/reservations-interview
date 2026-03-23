using System.Text.Json.Serialization;

namespace Contracts
{
    public record ErrorResponse(
        string Title,
        string Detail,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ResourceType = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ResourceId = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Dictionary<string, string[]>? Errors = null
    );
}
