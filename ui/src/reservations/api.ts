import { useQuery } from "@tanstack/react-query";
import ky from "ky";
import { z } from "zod";

export interface NewReservation {
  RoomNumber: string;
  GuestEmail: string;
  Start: Date;
  End: Date;
}

/** The schema the API returns */
const ReservationSchema = z.object({
  Id: z.string(),
  RoomNumber: z.string(),
  GuestEmail: z.string().email(),
  Start: z.string(),
  End: z.string(),
});

type Reservation = z.infer<typeof ReservationSchema>;

export function bookRoom(booking: NewReservation) {
  const newReservation = {
    ...booking,
    Start: toDateOnlyStr(booking.Start),
    End: toDateOnlyStr(booking.End),
  };

  return ky
    .post("api/reservation", {
      json: newReservation,
    })
    .json<Reservation>();
}

const RoomSchema = z.object({
  number: z.string(),
  state: z.number(),
});

const RoomListSchema = RoomSchema.array();

export function useGetRooms() {
  return useQuery({
    queryKey: ["rooms"],
    queryFn: () => ky.get("api/room").json().then(RoomListSchema.parseAsync),
  });
}

function toDateOnlyStr(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}
