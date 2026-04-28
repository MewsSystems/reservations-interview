namespace Models.Errors
{
    public class InvalidBooking : Exception
    {
        public InvalidBooking(string reason)
            : base(reason) { }
    }
}
