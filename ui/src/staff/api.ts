import { useQuery } from "@tanstack/react-query";
import ky from "ky";
import { z } from "zod";

const staffClient = ky.create({
  credentials: "same-origin",
});

const StaffReservationSchema = z.object({
  id: z.string(),
  roomNumber: z.string(),
  guestEmail: z.string().email(),
  start: z.string(),
  end: z.string(),
  checkedIn: z.boolean(),
  checkedOut: z.boolean(),
});

const StaffReservationListSchema = StaffReservationSchema.array();

export type StaffReservation = z.infer<typeof StaffReservationSchema>;

export async function checkStaffSession() {
  const response = await staffClient.get("api/staff/check", {
    throwHttpErrors: false,
  });

  return response.ok;
}

export async function loginStaff(accessCode: string) {
  await staffClient.get("api/staff/login", {
    headers: {
      "X-Staff-Code": accessCode,
    },
  });

  return checkStaffSession();
}

export function useGetStaffReservations(enabled: boolean) {
  return useQuery({
    queryKey: ["staff", "reservations"],
    enabled,
    retry: false,
    queryFn: () =>
      staffClient
        .get("api/reservation")
        .json()
        .then(StaffReservationListSchema.parseAsync),
  });
}
