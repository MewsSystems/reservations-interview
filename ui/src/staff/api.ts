import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import ky from "ky";
import { z } from "zod";

export async function staffLogin(accessCode: string): Promise<boolean> {
  try {
    await ky.get("/api/staff/login", {
      headers: { "X-Staff-Code": accessCode },
    });
    return true;
  } catch (err) {
    return false;
  }
}

const ReservationSchema = z.object({
  id: z.string(),
  roomNumber: z.string(),
  guestEmail: z.string(),
  start: z.string(),
  end: z.string(),
  checkedIn: z.boolean(),
  checkedOut: z.boolean(),
});

export type StaffReservation = z.infer<typeof ReservationSchema>;

const ReservationListSchema = ReservationSchema.array();

export function useGetStaffReservations() {
  return useQuery({
    queryKey: ["staff", "reservations"],
    queryFn: () =>
      ky.get("/api/staff/reservations").json().then(ReservationListSchema.parseAsync),
    retry: false,
  });
}

export function useCheckIn() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ reservationId, guestEmail }: { reservationId: string; guestEmail: string }) =>
      ky.post(`/api/reservation/${reservationId}/checkin`, {
        json: { guestEmail },
      }).json(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["staff", "reservations"] });
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    },
  });
}

// 0 = Ready, 1 = Occupied, 2 = Dirty
export enum RoomState {
  Ready = 0,
  Occupied = 1,
  Dirty = 2,
}

const RoomSchema = z.object({
  number: z.string(),
  state: z.number(),
});

const RoomListSchema = RoomSchema.array();
export type StaffRoom = z.infer<typeof RoomSchema>;

export function useGetRoomsState() {
  return useQuery({
    queryKey: ["rooms"],
    queryFn: () => ky.get("/api/room").json().then(RoomListSchema.parseAsync),
  });
}

export function useSetRoomState() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ roomNumber, state }: { roomNumber: string; state: RoomState }) =>
      ky.put(`/api/room/${roomNumber}/state`, { json: { state } }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    },
  });
}
