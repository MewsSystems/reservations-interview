import { useQuery } from "@tanstack/react-query";
import { api } from "../utils/api-client";
import { z } from "zod";

export async function staffLogin(accessCode: string): Promise<string> {
  const response = await api
    .post("/api/staff/login", {
      headers: { "X-Staff-Code": accessCode },
    })
    .json<{ token: string }>();

  return response.token;
}

const StaffReservationSchema = z.object({
  id: z.string(),
  roomNumber: z.string(),
  guestEmail: z.string(),
  start: z.string(),
  end: z.string(),
  checkedIn: z.boolean(),
  checkedOut: z.boolean(),
  checkedOutAt: z.string().nullable(),
});

export type StaffReservation = z.infer<typeof StaffReservationSchema>;

const StaffReservationListSchema = StaffReservationSchema.array();

export interface ReservationFilters {
  from?: string;
  to?: string;
  roomNumber?: string;
  guestEmail?: string;
}


const RoomStateLabels: Record<number, string> = {
  0: "Ready",
  1: "Occupied",
  2: "Dirty",
};

export { RoomStateLabels };

const ImportErrorSchema = z.object({
  line: z.number(),
  roomNumber: z.string(),
  message: z.string(),
});

const ImportResultSchema = z.object({
  totalRows: z.number(),
  imported: z.number(),
  failed: z.number(),
  errors: ImportErrorSchema.array(),
});

export type ImportResult = z.infer<typeof ImportResultSchema>;
export type ImportError = z.infer<typeof ImportErrorSchema>;

export async function importRooms(token: string, file: File): Promise<ImportResult> {
  const formData = new FormData();
  formData.append("file", file);

  return api
    .post("/api/rooms/import", {
      body: formData,
      headers: { Authorization: `Bearer ${token}` },
    })
    .json()
    .then(ImportResultSchema.parseAsync);
}

export async function updateRoomState(token: string, roomNumber: string, state: number) {
  return api
    .patch(`/api/rooms/${roomNumber}`, {
      json: { state },
      headers: { Authorization: `Bearer ${token}` },
    })
    .json();
}

export async function deleteRoom(token: string, roomNumber: string) {
  return api
    .delete(`/api/rooms/${roomNumber}`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    .json();
}

export function useGetStaffReservations(token: string | null, filters: ReservationFilters) {
  const searchParams: Record<string, string> = {};
  if (filters.from) searchParams.from = filters.from;
  if (filters.to) searchParams.to = filters.to;
  if (filters.roomNumber) searchParams.roomNumber = filters.roomNumber;
  if (filters.guestEmail) searchParams.guestEmail = filters.guestEmail;

  return useQuery({
    queryKey: ["staff-reservations", token, filters],
    enabled: !!token,
    retry: false,
    refetchOnWindowFocus: false,
    queryFn: () =>
      api
        .get("/api/reservations", {
          searchParams,
          headers: { Authorization: `Bearer ${token}` },
        })
        .json()
        .then(StaffReservationListSchema.parseAsync),
  });
}

export async function checkInReservation(token: string, reservationId: string, guestEmail: string) {
  return api
    .post(`/api/reservations/${reservationId}/check-in`, {
      json: { guestEmail },
      headers: { Authorization: `Bearer ${token}` },
    })
    .json();
}

export async function checkOutReservation(token: string, reservationId: string, guestEmail: string) {
  return api
    .post(`/api/reservations/${reservationId}/check-out`, {
      json: { guestEmail },
      headers: { Authorization: `Bearer ${token}` },
    })
    .json();
}