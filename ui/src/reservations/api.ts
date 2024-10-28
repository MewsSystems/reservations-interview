import { useQuery } from "@tanstack/react-query";
import { fromDateStringToIso, ISO8601String, toIsoStr } from "../utils/datetime";
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
  guestEmail: z.string(),
  start: z.string(),
  end: z.string(),
});

const ReservationListSchema = ReservationSchema.array();

export function bookRoom(booking: NewReservation) {
  // unwrap branded types
  const newReservation = {
    ...booking,
    Start: toIsoStr(booking.Start),
    End: toIsoStr(booking.End),
  };

  return ky.post("api/reservation", { json: newReservation });
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

export function useGetReservationsFromToday() {
  const dateNow = new Date();
  return useQuery({
    queryKey: [`reservationsFromToday-${dateNow}`],
    queryFn: () => ky.get(`api/reservation?fromDate=${toIsoStr(fromDateStringToIso(dateNow.toDateString()))}`).json().then(ReservationListSchema.parseAsync),
  });
}
