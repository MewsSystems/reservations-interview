import { useQuery } from "@tanstack/react-query";
import ky from "ky";
import { z } from "zod";

export interface NewReservation {
  RoomNumber: string;
  GuestEmail: string;
  Start: Date | null;
  End: Date | null;
}

/** The schema the API returns */
const ReservationSchema = z.object({
  Id: z.string(),
  RoomNumber: z.string(),
  GuestEmail: z.string().email(),
  Start: z.string().nullable(),
  End: z.string().nullable(),
});

type Reservation = z.infer<typeof ReservationSchema>;

export function bookRoom(booking: NewReservation) {
  // unwrap branded types
  const newReservation = {
    ...booking,
    Start: booking.Start ? toDateStr(booking.Start) : null,
    End: booking.End ? toDateStr(booking.End) : null,
  };

 return ky.post("api/reservation", { json: newReservation }).json<Reservation>();
}

const RoomSchema = z.object({
  number: z.string(),
  state: z.number(),
});

function toDateStr(date: Date | string): string {
  const d = new Date(date);
  
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

const RoomListSchema = RoomSchema.array();

export function useGetRooms() {
  return useQuery({
    queryKey: ["rooms"],
    queryFn: () => ky.get("api/room").json().then(RoomListSchema.parseAsync),
  });
}
