using api.Shared.Extensions;
using api.Shared.Models.Domain;
using api.Shared.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace api.Shared.Services
{
    public class GuestService : IGuestService
    {
        private readonly ILogger<GuestService> _logger;
        private readonly IGuestRepository _repository;

        public GuestService(ILogger<GuestService> logger, IGuestRepository repository)
        {
            _logger = logger.ThrowIfNull(nameof(ILogger<GuestService>));
            _repository = repository.ThrowIfNull(nameof(IGuestRepository));
        }

        public async Task<IEnumerable<Guest>> Get()
        {
            return (await _repository.GetGuests()).ToDomain();
        }

        public async Task<Guest> GetByEmail(string guestEmail)
        {
            return (await _repository.GetGuestByEmail(guestEmail)).ToDomain();
        }

        public async Task<Guest> Create(Guest newGuest, IDbTransaction? transaction = null)
        {
            var result = (await _repository.CreateGuest(newGuest.FromDomain(), transaction)).ToDomain();
            _logger?.LogInformation("New guest <{@email}> created.", result.Email);
            return result;
        }

        public async Task<bool> DeleteByEmail(string guestEmail)
        {
            var result = await _repository.DeleteGuestByEmail(guestEmail);
            if (result)
                _logger?.LogInformation("Guest <{@email}> deleted.", guestEmail);
            return result;
        }

        public async Task<bool> ConfirmAccount(string guestEmail)
        {
            await _repository.GetGuestByEmail(guestEmail);
            var result = await _repository.ConfirmAccount(guestEmail);
            if (result)
                _logger?.LogInformation("Guest <{@email}> email confirmed.", guestEmail);
            return result;
        }

    }
}
