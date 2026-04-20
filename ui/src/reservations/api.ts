import { useQuery } from "@tanstack/react-query";
import { ISO8601String, toIsoStr } from "../utils/datetime";
import ky, { HTTPError } from "ky";
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

export type Reservation = z.infer<typeof ReservationSchema>;

export class BookingError extends Error {
  constructor(public readonly messages: string[]) {
    super(messages.join(", "));
  }
}

export async function bookRoom(booking: NewReservation): Promise<Reservation> {
  // unwrap branded types
  const newReservation = {
    ...booking,
    Start: toIsoStr(booking.Start),
    End: toIsoStr(booking.End),
  };

  try {
    return await ky.post("api/reservation", { json: newReservation }).json<Reservation>();
  } catch (error) {
    if (error instanceof HTTPError && (error.response.status === 400 || error.response.status === 409)) {
      const text = await error.response.text();
      let body: unknown;
      try {
        body = JSON.parse(text);
      } catch {
        throw new BookingError([text]);
      }
      if (body && typeof body === "object" && "errors" in body) {
        const messages = Object.values(body.errors as Record<string, string[]>).flat();
        throw new BookingError(messages);
      }
      if (typeof body === "string") {
        throw new BookingError([body]);
      }
    }
    throw error;
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

const ReservationListSchema = ReservationSchema.array();

export function useGetUpcomingReservations(options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: ["reservations", "upcoming"],
    queryFn: async () => {
      const raw = await ky.get("api/reservation/upcoming").json<unknown>();
      return ReservationListSchema.parseAsync(raw);
    },
    enabled: options?.enabled,
  });
}

export function useGetRoomReservations(roomNumber: string) {
  return useQuery({
    queryKey: ["reservations", roomNumber],
    queryFn: async () => {
      const raw = await ky.get(`api/reservation/room/${roomNumber}`).json<unknown>();
      return ReservationListSchema.parseAsync(raw);
    },
    enabled: !!roomNumber,
  });
}
