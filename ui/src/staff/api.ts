import ky from "ky";

export type StaffReservation = {
  id: string;
  roomNumber: string;
  guestEmail: string;
  start: string;
  end: string;
};

export async function loginAsStaff(accessCode: string) {
  await ky.get("api/staff/login", {
    credentials: "same-origin",
    headers: { "X-Staff-Code": accessCode },
  });
}

export function getStaffReservations() {
  return ky
    .get("api/staff/reservations", {
      credentials: "same-origin",
    })
    .json<StaffReservation[]>();
}
