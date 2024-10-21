import { useQuery } from "@tanstack/react-query";
import { ISO8601String, toIsoStr } from "../utils/datetime";
import ky from "ky";
import { z } from "zod";

export interface NewReservation {
  RoomNumber: string;
  GuestEmail: string;
  Start: ISO8601String;
  End: ISO8601String;
}

/** The schema the API returns */
const ReservationSchema = z.object({
  id: z.string(),
  roomNumber: z.string(),
  guestEmail: z.string().email(),
  start: z.string(),
  end: z.string(),
});

type Reservation = z.infer<typeof ReservationSchema>;

export async function bookRoom(booking: NewReservation) {
  // unwrap branded types
  const newReservation = {
    ...booking,
    Start: toIsoStr(booking.Start),
    End: toIsoStr(booking.End),
  };

  var json = await ky.post("api/reservation", { json: newReservation }).json();
  console.log("here");
  var parsed = await ReservationSchema.parseAsync(json);

  return Promise.resolve<Reservation>(parsed as any as Reservation);
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
