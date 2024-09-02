using api.Shared.Extensions;
using api.Shared.Models.Domain;
using api.Shared.Models.Errors;
using api.Shared.Repositories;
using api.Shared.Validation.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace api.Shared.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ILogger<ReservationService> _logger;
        private readonly IGuestService _guestService;
        private readonly IDbConnection _connection;
        private readonly IRoomService _roomService;
        private readonly IReservationRepository _repository;

        public ReservationService(ILogger<ReservationService> logger,
            IGuestService service,
            IDbConnection connection,
            IRoomService roomService,
            IReservationRepository repository)
        {
            _logger = logger.ThrowIfNull(nameof(ILogger<ReservationService>));
            _guestService = service.ThrowIfNull(nameof(IGuestService));
            _connection = connection.ThrowIfNull(nameof(IDbConnection));
            _roomService = roomService.ThrowIfNull(nameof(IRoomService));
            _repository = repository.ThrowIfNull(nameof(IReservationRepository));
        }

        public async Task<IEnumerable<Reservation>> Get()
        {
            return (await _repository.GetReservations()).ToDomain();
        }

        public async Task<IEnumerable<Reservation>> GetStaffReservations()
        {
            return (await _repository.GetStaffReservations()).ToDomain();
        }

        public async Task<Reservation> GetByReservationId(Guid reservationId)
        {
            return (await _repository.GetReservation(reservationId)).ToDomain();
        }

        public async Task<Reservation> Create(Reservation reservation, IDbTransaction? transaction = null)
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
            using var trans = transaction ?? _connection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                Guest? guest = null;
                try
                {
                    guest = await _guestService.GetByEmail(reservation.GuestEmail);
                }
                catch (NotFoundException)
                {
                    guest = await _guestService.Create(new Guest() { Email = reservation.GuestEmail, Name = reservation.GuestEmail }, trans);
                }

                var validation = await new ReservationValidator().ValidateAsync(reservation);
                if (!validation.IsValid)
                {
                    throw new ServiceValidationException(validation.Errors);
                }
                var result = (await _repository.CreateReservation(reservation.FromDomain(), trans)).ToDomain();
                _logger?.LogInformation("New reservation <{@id}> created.", reservation.Id);
                trans.Commit();
                return result;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public async Task<bool> Delete(Guid reservationId)
        {
            var result = await _repository.DeleteReservation(reservationId);
            if (result)
                _logger?.LogInformation("Reservation <{@id}> created.", reservationId);
            return result;
        }

        public async Task<bool> CheckIn(Guid reservationId, string emailAddress)
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
            using var transaction = _connection.BeginTransaction();
            try
            {
                var reservation = await GetByReservationId(reservationId);
                if (reservation.GuestEmail != emailAddress)
                    throw new NotFoundException($"Reservation not found for guest email address {emailAddress}!");
                if (reservation.CheckedIn)
                    throw new BadRequestException("Guest already checked in.");
                var roomStatusResult = await _roomService.UpdateRoomStatus(reservation.RoomNumber, Constants.State.Occupied, transaction);
                var result = roomStatusResult ? await _repository.CheckIn(reservationId, emailAddress, transaction) : roomStatusResult;
                if (!result)
                    throw new Exception("Check in failed!");
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
