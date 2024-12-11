import React, { useEffect, useState } from "react";
import { Box, Heading } from "@radix-ui/themes";
import "../styles/UpcomingReservation.css";


export function UpcomingReservations() {
  const [reservations, setReservations] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch("/api/reservation/upcoming")
      .then((response) => response.json())
      .then((data) => {
        setReservations(data);
        setLoading(false);
      })
      .catch((error) => {
        console.error("Error fetching upcoming reservations:", error);
        setLoading(false);
      });
  }, []);

  if (loading) {
    return <p>Loading...</p>;
  }

  return (
    <Box p="4">
      <Heading size="8" as="h1" color="indigo">
        Upcoming Reservations
      </Heading>

      {reservations.length > 0 ? (
        <table className="reservation-table">
          <thead>
            <tr>
              <th>Room Number</th>
              <th>Guest Email</th>
              <th>Start</th>
              <th>End</th>
              <th>Checked In</th>
              <th>Checked Out</th>
            </tr>
          </thead>
          <tbody>
            {reservations.map((reservation) => (
              <tr key={reservation.id}>
                <td>{reservation.roomNumber}</td>
                <td>{reservation.guestEmail}</td>
                <td>{new Date(reservation.start).toLocaleString()}</td>
                <td>{new Date(reservation.end).toLocaleString()}</td>
                <td>{reservation.checkedIn ? "Yes" : "No"}</td>
                <td>{reservation.checkedOut ? "Yes" : "No"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <p>No upcoming reservations.</p>
      )}
    </Box>
  );
}
