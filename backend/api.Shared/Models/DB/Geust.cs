namespace api.Shared.Models.DB
{
    public class Guest
    {
        public required string Email { get; set; }

        public required string Name { get; set; }

        public string? Surname { get; set; }
    }
}
