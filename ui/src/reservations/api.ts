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


const ReservationSchema = z.object({
  Id: z.string(),
  RoomNumber: z.string(),
  GuestEmail: z.string().email(),
  Start: z.string(),
  End: z.string(),
});

type Reservation = z.infer<typeof ReservationSchema>;


export async function bookRoom(booking: NewReservation): Promise<Reservation> {
  const newReservation = {
    ...booking,
    Start: toIsoStr(booking.Start),
    End: toIsoStr(booking.End),
  };

  try {
    const response = await ky.post('api/reservation', {
      json: newReservation,
    });

    if (!response.ok) {
      throw new Error('Failed to book reservation');
    }

    const createdReservation: Reservation = await response.json();

    return createdReservation;
  } catch (error) {
    console.error('Error booking reservation:', error);
    
    throw new Error('Error booking reservation');
  }
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
