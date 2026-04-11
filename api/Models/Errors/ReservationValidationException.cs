namespace Models.Errors
{
    public class ReservationValidationException : Exception
    {
        public ReservationValidationException(string message)
            : base(message) { }
    }
}
