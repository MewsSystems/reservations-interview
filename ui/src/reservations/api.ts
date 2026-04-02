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

/** The schema the API returns (camelCase — ASP.NET Core default) */
const ReservationSchema = z.object({
  id: z.string(),
  roomNumber: z.string(),
  guestEmail: z.string(),
  start: z.string(),
  end: z.string(),
});

type Reservation = z.infer<typeof ReservationSchema>;

export async function bookRoom(booking: NewReservation): Promise<Reservation> {
  const newReservation = {
    roomNumber: booking.RoomNumber,
    guestEmail: booking.GuestEmail,
    start: toIsoStr(booking.Start),
    end: toIsoStr(booking.End),
  };

  const response = await ky.post("/api/reservation", { json: newReservation });
  const data = await response.json();
  return ReservationSchema.parse(data);
}

const RoomSchema = z.object({
  number: z.string(),
  state: z.number(),
  isDirty: z.boolean(),
});

export type Room = z.infer<typeof RoomSchema>;

const RoomListSchema = RoomSchema.array();

export function useGetRooms() {
  return useQuery({
    queryKey: ["rooms"],
    queryFn: () => ky.get("/api/room").json().then(RoomListSchema.parseAsync),
  });
}

const ReservationDetailSchema = z.object({
  id: z.string(),
  roomNumber: z.string(),
  guestEmail: z.string(),
  start: z.string(),
  end: z.string(),
  checkedIn: z.boolean(),
  checkedOut: z.boolean(),
});

export type ReservationDetail = z.infer<typeof ReservationDetailSchema>;

const ReservationListSchema = ReservationDetailSchema.array();

export interface PaginatedReservations {
  items: ReservationDetail[];
  totalCount: number;
  page: number;
  pageSize: number;
}

function toLocalDateStr(date: Date): string {
  const yyyy = date.getFullYear();
  const mm = String(date.getMonth() + 1).padStart(2, "0");
  const dd = String(date.getDate()).padStart(2, "0");
  return `${yyyy}-${mm}-${dd}`;
}

export function useGetUpcomingReservations(
  page = 1,
  pageSize = 20,
  todayOnly = false,
) {
  const from = toLocalDateStr(new Date());
  const searchParams: Record<string, string | number> = { from, page, pageSize };
  if (todayOnly) searchParams.to = from;

  return useQuery({
    queryKey: ["reservations", "upcoming", from, page, pageSize, todayOnly],
    queryFn: async (): Promise<PaginatedReservations> => {
      const response = await ky.get("/api/reservation", { searchParams });
      const items = ReservationListSchema.parse(await response.json());
      return {
        items,
        totalCount: Number(response.headers.get("X-Total-Count") ?? "0"),
        page: Number(response.headers.get("X-Page") ?? "1"),
        pageSize: Number(response.headers.get("X-Page-Size") ?? "20"),
      };
    },
    retry: false,
  });
}

const InitiateCheckInResponseSchema = z.object({
  code: z.string(),
});

export async function initiateCheckIn(reservationId: string): Promise<string> {
  const response = await ky.post(`/api/reservation/${reservationId}/checkin`);
  const data = InitiateCheckInResponseSchema.parse(await response.json());
  return data.code;
}

export async function confirmCheckIn(
  reservationId: string,
  code: string,
): Promise<void> {
  await ky.put(`/api/reservation/${reservationId}/checkin`, {
    json: { code },
  });
}

export interface ImportResult {
  imported: number;
  errors: { row: number; message: string }[];
}

export async function importRoomsCsv(file: File): Promise<ImportResult> {
  const formData = new FormData();
  formData.append("file", file);
  const response = await ky.post("/api/room/import", { body: formData });
  return (await response.json()) as ImportResult;
}

export async function updateRoomDirtyState(
  roomNumber: string,
  isDirty: boolean,
): Promise<Room> {
  const response = await ky.patch(`/api/room/${roomNumber}`, {
    json: [{ op: "replace", path: "/isDirty", value: isDirty }],
  });
  return RoomSchema.parse(await response.json());
}
