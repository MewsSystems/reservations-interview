import { useQuery } from "@tanstack/react-query";
import { ISO8601String, toIsoStr } from "../utils/datetime";
import { HTTPError } from "ky";
import { api } from "../utils/api-client";
import { z } from "zod";

export interface NewReservation {
  roomNumber: string;
  guestEmail: string;
  start: ISO8601String;
  end: ISO8601String;
}

/**  ----- The schemas the API returns ---- */

const ReservationSchema = z.object({
  id: z.string(),
  roomNumber: z.string(),
  guestEmail: z.string().email(),
  start: z.string(),
  end: z.string(),
});

export type Reservation = z.infer<typeof ReservationSchema>;

const ErrorResponseSchema = z.object({
  title: z.string(),
  detail: z.string(),
  resourceType: z.string().nullish(),
  resourceId: z.string().nullish(),
  errors: z.record(z.array(z.string())).nullish(),
});

export type ErrorResponse = z.infer<typeof ErrorResponseSchema>;

const RoomSchema = z.object({
  number: z.string(),
  state: z.number(),
});

export type Room = z.infer<typeof RoomSchema>;

const RoomListSchema = RoomSchema.array();


/**----- API  ---- */

export async function bookRoom(booking: NewReservation): Promise<Reservation> {
  const body = {
    ...booking,
    start: toIsoStr(booking.start),
    end: toIsoStr(booking.end),
  };

  return api
    .post("/api/reservations", { json: body })
    .json()
    .then(ReservationSchema.parseAsync);
}

export async function parseApiError(error: unknown): Promise<string[]> {
  if (error instanceof HTTPError) {
    try {
      const body = await error.response.json();
      const parsed = ErrorResponseSchema.safeParse(body);
      if (parsed.success) {
        if (parsed.data.errors) {
          return Object.values(parsed.data.errors).flat();
        }
        return [parsed.data.detail];
      }
    } catch {
      // fall through
    }
  }
  return ["An unexpected error occurred"];
}

export function useGetRooms() {
  return useQuery({
    queryKey: ["rooms"],
    queryFn: () => api.get("/api/rooms").json().then(RoomListSchema.parseAsync),
  });
}
