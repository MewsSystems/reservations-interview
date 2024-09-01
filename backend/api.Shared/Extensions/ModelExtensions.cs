using System;
using System.Collections.Generic;

namespace api.Shared.Extensions
{
    /// <summary>
    /// Mapping done in extension methods. If domain logic grows larger move to mapping nuget such as AutoMapper.
    /// </summary>
    public static class ModelExtensions
    {
        #region Reservation

        public static Models.DB.Reservation FromDomain(this Models.Domain.Reservation reservation)
        {
            if (reservation == null)
                throw new ArgumentNullException($"Cannot map null <{nameof(Models.DB.Reservation)}> object to db object.");
            return new Models.DB.Reservation {
                Id = reservation.Id.ToString(),
                RoomNumber = reservation.RoomNumber.ConvertRoomNumberToInt(),
                GuestEmail = reservation.GuestEmail,
                Start = reservation.Start,
                End = reservation.End,
                CheckedIn = reservation.CheckedIn,
                CheckedOut = reservation.CheckedOut
            };
        }

        public static Models.Domain.Reservation ToDomain(this Models.DB.Reservation reservation)
        {
            if (reservation == null)
                throw new ArgumentNullException($"Cannot map null <{nameof(Models.DB.Reservation)}> object to domain object.");
            return new Models.Domain.Reservation
            {
                Id = Guid.Parse(reservation.Id),
                RoomNumber = reservation.RoomNumber.FormatRoomNumber(),
                GuestEmail = reservation.GuestEmail,
                Start = reservation.Start,
                End = reservation.End,
                CheckedIn = reservation.CheckedIn,
                CheckedOut = reservation.CheckedOut
            };
        }
        public static IEnumerable<Models.Domain.Reservation> ToDomain(this IEnumerable<Models.DB.Reservation> reservations)
        {
            var result = new List<Models.Domain.Reservation>();
            if (reservations == null)
                return result;
            foreach (var reservation in reservations)
            {
                result.Add(ToDomain(reservation));
            }
            return result;
        }

        #endregion Reservation

        #region Room

        public static Models.DB.Room FromDomain(this Models.Domain.Room room)
        {
            if (room == null)
                throw new ArgumentNullException($"Cannot map null <{nameof(Models.Domain.Room)}> object to db object.");
            return new Models.DB.Room { Number = room.Number.ConvertRoomNumberToInt(), State = room.State };
        }

        public static Models.Domain.Room ToDomain(this Models.DB.Room room)
        {
            if (room == null)
                throw new ArgumentNullException($"Cannot map null <{nameof(Models.DB.Room)}> object to domain object.");
            return new Models.Domain.Room { Number = room.Number.FormatRoomNumber(), State = room.State };
        }

        public static IEnumerable<Models.Domain.Room> ToDomain(this IEnumerable<Models.DB.Room> rooms)
        {
            var result = new List<Models.Domain.Room>();
            if (rooms == null)
                return result;
            foreach (var item in rooms) 
            {
                var domain = ToDomain(item);
                if(domain != null)
                    result.Add(domain);
            }
            return result;
        }

        #endregion Room

        #region Guest

        public static Models.DB.Guest FromDomain(this Models.Domain.Guest guest)
        {
            if (guest == null)
                throw new ArgumentNullException($"Cannot map null <{nameof(Models.Domain.Guest)}> object to db object.");
            return new Models.DB.Guest { Email = guest.Email, Name = guest.Name, Surname = guest.Surname };
        }

        public static Models.Domain.Guest ToDomain(this Models.DB.Guest guest)
        {
            if (guest == null)
                throw new ArgumentNullException($"Cannot map null <{nameof(Models.DB.Guest)}> object to domain object.");
            return new Models.Domain.Guest { Email = guest.Email, Name = guest.Name, Surname = guest.Surname };
        }

        public static IEnumerable<Models.Domain.Guest> ToDomain(this IEnumerable<Models.DB.Guest> rooms)
        {
            var result = new List<Models.Domain.Guest>();
            if (rooms == null)
                return result;
            foreach (var item in rooms)
            {
                var domain = ToDomain(item);
                if (domain != null)
                    result.Add(domain);
            }
            return result;
        }

        #endregion Guest

    }
}
