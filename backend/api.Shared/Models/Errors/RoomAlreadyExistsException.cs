using System;

namespace api.Shared.Models.Errors
{
    public class RoomAlreadyExistsException : Exception
    {
        public RoomAlreadyExistsException(string roomNumber)
            : base($"Room with same number {roomNumber} already exists") { }
    }
}
