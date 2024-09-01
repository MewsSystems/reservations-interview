using api.Shared.Extensions;
using api.Shared.Models.Domain;
using api.Shared.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Shared.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ILogger<ReservationService> _logger;
        private readonly IReservationRepository _repository;

        public ReservationService(ILogger<ReservationService> logger, IReservationRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<IEnumerable<Reservation>> Get()
        {
            return (await _repository.GetReservations()).ToDomain();
        }

        public async Task<Reservation> GetByReservationId(Guid reservationId)
        {
            return (await _repository.GetReservation(reservationId)).ToDomain();
        }

        public async Task<Reservation> Create(Reservation reservation)
        {
            var result = (await _repository.CreateReservation(reservation.FromDomain())).ToDomain();
            _logger?.LogInformation("New reservation <{@id}> created.", reservation.Id);
            return result;
        }

        public async Task<bool> Delete(Guid reservationId)
        {
            var result = await _repository.DeleteReservation(reservationId);
            if (result)
                _logger?.LogInformation("Reservation <{@id}> created.", reservationId);
            return result;
        }
    }
}
