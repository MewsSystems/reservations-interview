using api.Shared.Extensions;
using api.Shared.Models.Domain;
using api.Shared.Models.Errors;
using api.Shared.Repositories;
using api.Shared.Validation.Domain;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Shared.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ILogger<ReservationService> _logger;
        private readonly IGuestService _guestService;
        private readonly IReservationRepository _repository;

        public ReservationService(ILogger<ReservationService> logger,
            IGuestService service,
            IReservationRepository repository)
        {
            _logger = logger;
            _guestService = service;
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
            Guest? guest = null;
            try
            {
                guest = await _guestService.GetByEmail(reservation.GuestEmail);
            }
            catch (NotFoundException)
            {
                guest = await _guestService.Create(new Guest() { Email = reservation.GuestEmail, Name = reservation.GuestEmail });
            }

            var validation = await new ReservationValidator().ValidateAsync(reservation);
            if(!validation.IsValid)
            {
                throw new ServiceValidationException(validation.Errors);
            }
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
