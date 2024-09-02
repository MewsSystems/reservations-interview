import { useQuery } from "@tanstack/react-query";
import ky from "ky";
import { z } from "zod";

export async function login(accessCode: string, onSuccess: () => void, onError: () => void) {
    return ky.get('/api/staff/login', {
        headers: {
            'X-Staff-Code': accessCode,
        },
    })
        .then(() => {
            onSuccess()
        })
        .catch(() => {
            onError()
        });
}

const GuestSchema = z.object({
    email: z.string(),
    name: z.string(),
    surname: z.string().nullable(),
    isValidated: z.boolean()
});

export async function getUserAccount(email: string) {
    return await ky.get(`/api/guest/${email}`, {
    }).json().then(GuestSchema.parseAsync)
}

const PatchReservationSchema = z.object({
    success: z.boolean()
});

export async function patchReservation(reservationId: string, email: string) {
    return ky.patch('/api/reservation', {
        headers: {
            'Content-Type': 'application/json'
        },
        json: {
            id: reservationId,
            guestEmail: email
        }
    }).json().then(PatchReservationSchema.parseAsync);
}

const StaffReservationSchema = z.object({
    id: z.string(),
    roomNumber: z.string(),
    guestEmail: z.string(),
    start: z.string().transform((str) => new Date(str)),
    end: z.string().transform((str) => new Date(str)),
    checkedIn: z.boolean(),
    checkedOut: z.boolean()
});

const StaffReservationSchemaListSchema = StaffReservationSchema.array();

export function useGetStaffReservations() {
    return useQuery({
        queryKey: ["staff-reservation"],
        queryFn: () => ky.get("/api/staff/reservation").json().then(StaffReservationSchemaListSchema.parseAsync),
    });
}