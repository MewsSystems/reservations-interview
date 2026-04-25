using Models;

namespace Validators.Interfaces
{
    public interface IReservationValidator
    {
        List<string> Validate(Reservation reservation);
    }
}
