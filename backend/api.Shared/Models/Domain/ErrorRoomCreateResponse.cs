namespace api.Shared.Models.Domain
{
    public class ErrorRoomCreateResponse
    {
        public required Room Room { get; set; }

        public required string ErrorMessage { get; set; }
    }
}
