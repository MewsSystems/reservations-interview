import { useQuery, useMutation } from "@tanstack/react-query";
import { ISO8601String, toIsoStr } from "../utils/datetime";
import ky from "ky";
import { z } from "zod";

export interface NewReservation {
    RoomNumber: string;
    GuestEmail: string;
    Name: string;
    Surname: string;
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

export function useStaffReservations(from?: string, to?: string) {
    return useQuery({
        queryKey: ["staffReservations", from, to],
        queryFn: async () => {
            const params = new URLSearchParams();

            if (from) params.append("from", from);
            if (to) params.append("to", to);

            return ky
                .get(`/api/staff/reservations?${params}`)
                .json()
                .then(ReservationSchema.array().parse);
        },
    });
}

export function useBookRoom() {
    return useMutation({
        mutationFn: bookRoom
    });
}

export async function bookRoom(booking: NewReservation) {
    // unwrap branded types
    const newReservation = {
        ...booking,
        Start: toIsoStr(booking.Start),
        End: toIsoStr(booking.End),
    };

    return await ky
        .post("/api/reservation", { json: newReservation })
        .json()
        .then(ReservationSchema.parse);
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
