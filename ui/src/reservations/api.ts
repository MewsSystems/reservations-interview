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

const StaffReservationSchema = z.object({
  Id: z.string(),
  RoomNumber: z.string(),
  GuestEmail: z.string().email(),
  Start: z.string(),
  End: z.string(),
  CheckedIn: z.boolean(),
  CheckedOut: z.boolean(),
});

type Reservation = z.infer<typeof ReservationSchema>;
const StaffReservationListSchema = StaffReservationSchema.array();

export async function staffLogin(code: string) {
  return ky.get("api/staff/login", {
    headers: { "X-Staff-Code": code }
  });
}

export function useGetStaffReservations() {
  return useQuery({
    queryKey: ["staff-reservations"],
    queryFn: () => 
      ky.get("api/staff/reservations")
        .json()
        .then(StaffReservationListSchema.parseAsync),
  });
}

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
  Number: z.string(),
  State: z.number(),
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
