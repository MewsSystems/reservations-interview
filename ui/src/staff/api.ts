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