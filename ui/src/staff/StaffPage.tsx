import { useEffect, useState } from "react";

type Reservation = {
  id: string;
  roomNumber: string;
  guestEmail: string;
  start: string;
  end: string;
};

export function StaffPage() {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetch("api/staff/reservations", {
      credentials: "include",
    })
      .then(async (res) => {
        if (res.status === 401) {
          setError("Not authorized");
          return;
        }

        const data = await res.json();
        setReservations(data);
      })
      .catch(() => setError("Failed to load reservations"));
  }, []);

  if (error) {
    return <div>{error}</div>;
  }

  return (
    <div style={{ padding: 20 }}>
      <h2>Upcoming Reservations</h2>

      {reservations.length === 0 && <div>No reservations</div>}

      <ul>
        {reservations.map((r) => (
          <li key={r.id}>
            <strong>Room:</strong> {r.roomNumber} <br />
            <strong>Email:</strong> {r.guestEmail} <br />
            <strong>From:</strong> {r.start} → <strong>To:</strong> {r.end}
          </li>
        ))}
      </ul>
    </div>
  );
}